/*
 * Copyright 2026-present Coinbase Global, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Security.Cryptography;
using System.Text;
using CoinbaseSdk.Core.Error;
using CoinbaseSdk.Prime.ApiKeyManagement;
using Xunit;

namespace CoinbaseSdk.Prime.Tests;

public class EncryptedCredentialsDecoderTests
{
  private const string SecretKey = "test-signing-key";
  private const string HkdfInfo = "api-key-rotation";

  [Fact]
  public void Decode_RoundTrip_ReturnsCredentials()
  {
    var payload = """
      {"access_key":"new-access","secret_key":"new-secret","passphrase":"new-pass","service_account_id":"sa-1"}
      """;
    var encrypted = Encrypt(SecretKey, payload);

    var creds = EncryptedCredentialsDecoder.Decode(encrypted, SecretKey);

    Assert.Equal("new-access", creds.AccessKey);
    Assert.Equal("new-secret", creds.SecretKey);
    Assert.Equal("new-pass", creds.Passphrase);
    Assert.Equal("sa-1", creds.ServiceAccountId);
  }

  [Fact]
  public void Decode_ResponseOverload_UsesEncryptedCredentials()
  {
    var payload = """{"access_key":"from-response","secret_key":"s","passphrase":"p"}""";
    var response = new RotateAPIKeyResponse
    {
      EncryptedCredentials = Encrypt(SecretKey, payload),
      ActivityId = "activity-1",
    };

    var creds = EncryptedCredentialsDecoder.Decode(response, SecretKey);

    Assert.Equal("from-response", creds.AccessKey);
  }

  [Fact]
  public void Decode_UnpaddedBase64_Succeeds()
  {
    var payload = """{"access_key":"k","secret_key":"s","passphrase":"p"}""";
    var encrypted = Encrypt(SecretKey, payload).TrimEnd('=');

    var creds = EncryptedCredentialsDecoder.Decode(encrypted, SecretKey);

    Assert.Equal("k", creds.AccessKey);
  }

  [Fact]
  public void Decode_UnsupportedVersion_Throws()
  {
    var encrypted = Encrypt(SecretKey, """{"access_key":"k"}""");
    var raw = Convert.FromBase64String(encrypted);
    raw[0] = 2;
    var tampered = Convert.ToBase64String(raw);

    var ex = Assert.Throws<CoinbaseClientException>(
      () => EncryptedCredentialsDecoder.Decode(tampered, SecretKey));
    Assert.Contains("wire version: 2", ex.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void Decode_WrongSecret_Throws()
  {
    var encrypted = Encrypt(SecretKey, """{"access_key":"k"}""");

    var ex = Assert.Throws<CoinbaseClientException>(
      () => EncryptedCredentialsDecoder.Decode(encrypted, "wrong-secret"));
    Assert.Contains("Failed to decrypt", ex.Message, StringComparison.Ordinal);
  }

  [Fact]
  public void Decode_MissingPayload_Throws()
  {
    Assert.Throws<CoinbaseClientException>(
      () => EncryptedCredentialsDecoder.Decode((string?)null, SecretKey));
  }

  [Fact]
  public void Decode_InvalidBase64_Throws()
  {
    Assert.Throws<CoinbaseClientException>(
      () => EncryptedCredentialsDecoder.Decode("not-base64!!!", SecretKey));
  }

  private static string Encrypt(string secretKey, string plaintextJson)
  {
    var salt = RandomNumberGenerator.GetBytes(32);
    var nonce = RandomNumberGenerator.GetBytes(12);
    var plaintext = Encoding.UTF8.GetBytes(plaintextJson);
    var ikm = Encoding.UTF8.GetBytes(secretKey);
    var aesKey = HKDF.DeriveKey(
      HashAlgorithmName.SHA256,
      ikm,
      32,
      salt,
      Encoding.UTF8.GetBytes(HkdfInfo));

    var ciphertext = new byte[plaintext.Length];
    var tag = new byte[16];
    using var aesGcm = new AesGcm(aesKey, 16);
    aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

    var raw = new byte[1 + salt.Length + nonce.Length + ciphertext.Length + tag.Length];
    raw[0] = 1;
    Buffer.BlockCopy(salt, 0, raw, 1, salt.Length);
    Buffer.BlockCopy(nonce, 0, raw, 1 + salt.Length, nonce.Length);
    Buffer.BlockCopy(ciphertext, 0, raw, 1 + salt.Length + nonce.Length, ciphertext.Length);
    Buffer.BlockCopy(tag, 0, raw, 1 + salt.Length + nonce.Length + ciphertext.Length, tag.Length);
    return Convert.ToBase64String(raw);
  }
}
