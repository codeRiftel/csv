.PHONY: build

build:
	mcs main.cs CSV.cs -langversion:ISO-2 -out:csv.exe
