SPEC_URL ?= https://api.prime.coinbase.com/v1/openapi.yaml
SPEC_FILE := apiSpec/prime-public-api-spec.yaml
SLN := prime-sdk-dotnet.sln

.PHONY: fetch-spec generate format format-fix
fetch-spec:
	@mkdir -p apiSpec
	curl -fsSL "$(SPEC_URL)" -o "$(SPEC_FILE)"

generate:
	dotnet run --project tools/generator
	$(MAKE) format-fix

format:
	dotnet format $(SLN) --verify-no-changes

format-fix:
	dotnet format $(SLN)
