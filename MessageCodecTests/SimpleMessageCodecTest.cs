using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageCodec;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Xunit;

namespace MessageEncoderTests
{
    public class SimpleMessageCodecTest
    {
        [Fact]
        public void Encode_Decode_ValidData_ReturnsOriginalMessage()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var originalMessage = new Message
            {
                headers = new Dictionary<string, string> {
                { "Content-Type", "text/plain" },
                { "X-Custom-Header", "CustomValue" },
                { "X-Another-Header", "AnotherValue" },
                { "X-Yet-Another-Header", "YetAnotherValue" }
            },
                payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }
            };

            // Act
            byte[] encodedMessage = codec.Encode(originalMessage);
            Message decodedMessage = codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(originalMessage.headers, decodedMessage.headers);
            Assert.Equal(originalMessage.payload, decodedMessage.payload);
        }

        [Fact]
        public void Encode_InvalidHeaderCount_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var messageWithTooManyHeaders = new Message
            {
                headers = new Dictionary<string, string>(),
                payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }
            };

            for (int i = 0; i < 65; i++)
            {
                messageWithTooManyHeaders.headers.Add($"X-Header-{i}", "value");
            }

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Encode(messageWithTooManyHeaders));
            Assert.Contains("Header count", exception.Message);
        }

        [Fact]
        public void Encode_InvalidHeaderKeyLength_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var messageWithLargeHeader = new Message
            {
                headers = new Dictionary<string, string> {
                { new string('K', 1024), "value" } // Key is too long
            },
                payload = new byte[] { }
            };

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Encode(messageWithLargeHeader));
            Assert.Contains("Header key or value length", exception.Message);
        }
        [Fact]
        public void Encode_InvalidHeaderValueLength_ThrowsArgumentException()
        {
            var codec = new SimpleMessageCodec();
            var messageWithLargeHeader = new Message
            {
                headers = new Dictionary<string, string> {
                { "key",new string('K', 1024)} // Value is too long
            },
                payload = new byte[] { }
            };

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Encode(messageWithLargeHeader));
            Assert.Contains("Header key or value length", exception.Message);
        }
        [Fact]
        public void Encode_InvalidPayloadLength_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var messageWithLargePayload = new Message
            {
                headers = new Dictionary<string, string>(),
                payload = new byte[257 * 1024]
            };
            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Encode(messageWithLargePayload));
            Assert.Contains("Payload length", exception.Message);

        }
        [Fact]
        public void Encode_EmptyMessage_ThrowsException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var emptyMessage = new Message
            {
                headers = new Dictionary<string, string>(),
                payload = new byte[] { }
            };

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Encode(emptyMessage));
            Assert.Contains("Message is empty", exception.Message);

        }
        [Fact]
        public void Decode_InvalidVersion_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var dataWithWrongVersion = new byte[] { 0xFF };

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Decode(dataWithWrongVersion));
            Assert.Contains("Unsupported version", exception.Message);
        }

        [Fact]
        public void Decode_ChecksumMismatch_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var validEncodedMessage = codec.Encode(new Message
            {
                headers = new Dictionary<string, string> { { "Test", "TestValue" } },
                payload = new byte[] { 0x01, 0x02, 0x03 }
            });

            validEncodedMessage[1] ^= 0xFF; // Flipping bits in the first byte of header count

            // Act / Assert
            var exception = Assert.Throws<ArgumentException>(() => codec.Decode(validEncodedMessage));
            Assert.Contains("Checksum mismatch", exception.Message);
        }
        [Fact]
        public void Decode_InvalidChecksum_ThrowsArgumentException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var encodedMessage = codec.Encode(new Message
            {
                headers = new Dictionary<string, string> { { "Header", "Value" } },
                payload = new byte[] { 0x00, 0x01, 0x02 }
            });
            // Flip the checksum
            encodedMessage[encodedMessage.Length - 1] ^= 0xff;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => codec.Decode(encodedMessage));
        }

        [Fact]
        public void Decode_EmptyPayload_ThrowsNoException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var encodedMessage = codec.Encode(new Message
            {
                headers = new Dictionary<string, string> { { "Header", "Value" } },
                payload = new byte[] { }
            });

            // Act & Assert (no exception should be thrown)
            var decodedMessage = codec.Decode(encodedMessage);
            Assert.Empty(decodedMessage.payload);
        }

        [Fact]
        public void Decode_InvalidDataFormat_ThrowsException()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var invalidData = new byte[] { 0x01, 0x00, 0x00 }; // Invalid, as there is no checksum byte

            // Act & Assert
            Assert.Throws<ArgumentException>(() => codec.Decode(invalidData));
        }



        [Fact]
        public void Encode_Decode_WithSpecialCharactersInHeaders_ReturnsOriginalMessage()
        {
            // Arrange
            var codec = new SimpleMessageCodec();
            var specialCharMessage = new Message
            {
                headers = new Dictionary<string, string> {
                { "Weird-Header", "ValueWithSpecialChars!@#$%^&*()" }
            },
                payload = new byte[] { 0x01, 0x02, 0x03 }
            };

            // Act
            var encodedMessage = codec.Encode(specialCharMessage);
            var decodedMessage = codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(specialCharMessage.headers, decodedMessage.headers);
            Assert.Equal(specialCharMessage.payload, decodedMessage.payload);
        }
    }
}