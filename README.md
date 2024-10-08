# CloudSeal

CloudSeal is a .NET application designed to securely encrypt files using AES-256 encryption. It provides a simple interface for creating compressed and encrypted 7z archives of files, ensuring the security and confidentiality of your data.

## Why It's Useful and What Problem It Solves

CloudSeal is particularly useful for encrypting data in folders that are synchronized with cloud storage services. By encrypting your files before they are uploaded to the cloud, you can ensure that sensitive information remains confidential and protected from unauthorized access, even if the cloud storage itself is compromised. This makes CloudSeal an ideal solution for anyone looking to securely store personal, business, or sensitive data in the cloud.


## Features

- Encrypt files with AES-256 encryption.
- Create compressed 7z archives.
- Command-line interface for ease of use.
- Secure file handling and encryption process.

## Installation

1. Build the project:

    ```bash
    dotnet build
    ```

2. Locate the two `.exe` files generated from the build. These are typically found in the `bin/Debug/net6.0` or `bin/Release/net6.0` directory.

3. Place both `.exe` files in any folder of your choice.

4. Run `CloudSeal.exe` to start the application.
