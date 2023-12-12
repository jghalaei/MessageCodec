# Simple Binary Encoder

## Description

This project implements a simple binary message encoding and decoding scheme tailored for a real-time communication signaling protocol. The codec is designed to handle messages with a variable number of ASCII-encoded name-value pair headers and a binary payload.

Key features of the codec include:

- Support for up to 63 headers per message.
- Header names and values are each limited to 1023 bytes.
- Support for a message payload of up to 256 KiB.
- Straightforward serialization and deserialization of message objects.
- Checksum for basic error detection.
- Versioning for openness to future changes

## High-Level Structure Overview

##### 1. Version (1 byte): To indicate the version of meesage's schema

##### 2. Header Count (1 byte): To represent up to 63 headers.

#### 3. Headers:

- Header Name Length (2 Bytes): Indicate length of Header Name.
- Header Name (n Bytes): ASCII encoded string.
- Header Name Length (2 Bytes): Indicate length of Header Value.
- Header Value (n Bytes): ASCII encoded string.

#### 4. Payload Length (4 Bytes): Indicates the length of the payload.

#### 5. Payload (n Bytes): The actual binary payload data

#### 6. Checksum (1 Byte): to verify the correctness of message.

## Message Structure Example:

    [Version (1B)][HeaderCount(1B)][HeaderNameLength(2B)][HeaderName(nB)][HeaderValueLength(2B)][HeaderValue(nB)]...[PayloadSize(4B)][PayloadData(nB)][Checksum(1B)]

## Design Rationale

#### Version:

Indicating version of message can be a valuable feature. As it will be very useful to easily change or improve our message schema in the future.

#### Header Count:

1 byte is enough to encode up to 255. Although our protocol limits us to 63 headers, 1 byte is chosen for simplicity and providing future flexibility.

#### Header Length (Name and Value each):

2 bytes provide up to 65535, but the requirement is only 1023 bytes. This is for future-proofing and ease of implementation.

#### Payload Length:

4 bytes allow us to encode up to ~4GiB, more than the 256KiB required.

#### Checksum

Adding a Checksum can be very useful. At it help to validate message before using.
For the sake of simplicity just a simple Checksum is used. which is the sum of all bytes modulo 255.

#### Simplicity:

Using fixed-length fields for the header count and lengths allows for simpler parsing and consistent overhead.

#### Extensibility:

- The current structure allows for extending the protocol with more headers or larger payloads without changing the overall schema.
- For applying more changes we can use the versioning feature, provided by adding the version on first of message.

## Assumptions:

- Message can not be empty. At least one header or payload should be exist.

## Error Handling:

The codec will check the following errors and in case of failing throws ArgumentException with proper message.

### Encoding

1. Invalid Header Count: Header counts more than 63.
2. Invalid Header Length: Header name or value length exceed the limit of 1023.
3. Payload size exceeds the limit of 256KiB.
4. Empty Message: Both Headers and payload are empty.

### Decoding

1. Invalid Version: The message version is not supported.
2. Checksum Mismatch: Indicating that the message is corrupted.
3. Invalid Checksum: Indicating that the message is corrupted.
