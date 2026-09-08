/*
 * Copyright 2026-present Coinbase Global, Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace CoinbaseSdk.Prime.ApiKeyManagement
{
  using System;
  using System.Security.Cryptography;
  using System.Text;
  using System.Text.Json;
  using CoinbaseSdk.Core.Error;
  using CoinbaseSdk.Prime.Serialization;

  /// <summary>
  /// Decrypts <see cref="RotateAPIKeyResponse.EncryptedCredentials"/> using the current
  /// API secret (signing key) and HKDF-SHA256 + AES-256-GCM. Uses only
  /// <c>System.Security.Cryptography</c> — no extra packages.
  /// </summary>
  /// <remarks>
  /// Wire format after Base64 decode:
  /// <c>version(1) | salt(32) | nonce(12) | ciphertext+tag</c>.
  /// Version must be <c>0x01</c>. HKDF info is <c>api-key-rotation</c>.
  /// The IKM is the UTF-8 bytes of the current secret string (the same value as
  /// <c>PRIME_SIGNING_KEY</c>), not the Base64-decoded HMAC key used for request signing.
  /// </remarks>
  public static class EncryptedCredentialsDecoder
  {
    private const string HkdfInfo = "api-key-rotation";
    private const byte SupportedVersion = 1;
    private const int VersionLen = 1;
    private const int SaltLen = 32;
    private const int NonceLen = 12;
    private const int TagLen = 16;
    private const int AesKeyLen = 32;
    private const int MinWireLen = VersionLen + SaltLen + NonceLen + TagLen;

    /// <summary>
    /// Decrypts credentials from a rotate-API-key response.
    /// </summary>
    /// <param name="response">Response containing <see cref="RotateAPIKeyResponse.EncryptedCredentials"/>.</param>
    /// <param name="currentSecretKey">Current API secret / signing key string.</param>
    public static RotatedApiKeyCredentials Decode(RotateAPIKeyResponse response, string currentSecretKey)
    {
      if (response == null)
      {
        throw new CoinbaseClientException("RotateAPIKeyResponse is required");
      }

      return Decode(response.EncryptedCredentials, currentSecretKey);
    }

    /// <summary>
    /// Decrypts a Base64-encoded <c>encrypted_credentials</c> payload.
    /// </summary>
    /// <param name="encryptedCredentials">Base64 wire payload from the rotate response.</param>
    /// <param name="currentSecretKey">Current API secret / signing key string.</param>
    public static RotatedApiKeyCredentials Decode(string? encryptedCredentials, string currentSecretKey)
    {
      if (string.IsNullOrWhiteSpace(encryptedCredentials))
      {
        throw new CoinbaseClientException("EncryptedCredentials is required");
      }

      if (string.IsNullOrEmpty(currentSecretKey))
      {
        throw new CoinbaseClientException("Current secret key is required");
      }

      byte[] raw;
      try
      {
        raw = DecodeBase64PadTolerant(encryptedCredentials);
      }
      catch (FormatException ex)
      {
        throw new CoinbaseClientException("EncryptedCredentials is not valid Base64", ex);
      }

      if (raw.Length < MinWireLen)
      {
        throw new CoinbaseClientException("EncryptedCredentials payload is truncated");
      }

      if (raw[0] != SupportedVersion)
      {
        throw new CoinbaseClientException($"Unsupported EncryptedCredentials wire version: {raw[0]}");
      }

      byte[] salt = raw.AsSpan(VersionLen, SaltLen).ToArray();
      byte[] nonce = raw.AsSpan(VersionLen + SaltLen, NonceLen).ToArray();
      ReadOnlySpan<byte> ciphertextAndTag = raw.AsSpan(VersionLen + SaltLen + NonceLen);
      byte[] ciphertext = ciphertextAndTag[..^TagLen].ToArray();
      byte[] tag = ciphertextAndTag[^TagLen..].ToArray();

      byte[] ikm = Encoding.UTF8.GetBytes(currentSecretKey);
      byte[] aesKey = HKDF.DeriveKey(
        HashAlgorithmName.SHA256,
        ikm,
        AesKeyLen,
        salt,
        Encoding.UTF8.GetBytes(HkdfInfo));
      CryptographicOperations.ZeroMemory(ikm);

      byte[] plaintext = new byte[ciphertext.Length];
      try
      {
        using var aesGcm = new AesGcm(aesKey, TagLen);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
      }
      catch (CryptographicException ex)
      {
        CryptographicOperations.ZeroMemory(plaintext);
        throw new CoinbaseClientException("Failed to decrypt EncryptedCredentials", ex);
      }
      finally
      {
        CryptographicOperations.ZeroMemory(aesKey);
        CryptographicOperations.ZeroMemory(salt);
        CryptographicOperations.ZeroMemory(ikm);
      }

      try
      {
        var creds = JsonSerializer.Deserialize<RotatedApiKeyCredentials>(
          plaintext,
          PrimeJsonSerializerOptionsFactory.Default);
        if (creds == null)
        {
          throw new CoinbaseClientException("Decrypted EncryptedCredentials JSON was empty");
        }

        return creds;
      }
      catch (JsonException ex)
      {
        throw new CoinbaseClientException("Decrypted EncryptedCredentials is not valid JSON", ex);
      }
      finally
      {
        CryptographicOperations.ZeroMemory(plaintext);
      }
    }

    private static byte[] DecodeBase64PadTolerant(string value)
    {
      var trimmed = value.Trim();
      var pad = (4 - (trimmed.Length % 4)) % 4;
      if (pad > 0)
      {
        trimmed = trimmed.PadRight(trimmed.Length + pad, '=');
      }

      return Convert.FromBase64String(trimmed);
    }
  }
}
