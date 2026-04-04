using Xunit;
using System.Collections.Generic;
using Legacy89DiskKit.Application.Services;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.CharacterEncoding.Application;

namespace Legacy89DiskKit.Tests
{
    public class FileNameNormalizationServiceTest
    {
        private readonly FileNameNormalizationService _service;
        private readonly EncoderRegistry _registry;

        public FileNameNormalizationServiceTest()
        {
            _registry = new EncoderRegistry();
            _registry.Register("X1", new X1CharacterEncoder());
            _registry.Register("SJIS", new ShiftJisCharacterEncoder());
            _service = new FileNameNormalizationService(_registry);
        }

        [Fact]
        public void Normalize_ShortName_Unchanged()
        {
            var result = _service.Normalize("SHORT.TXT", "X1", 8, 3);
            Assert.Equal("SHORT.TXT", result);
        }

        [Fact]
        public void Normalize_LongName_TruncatesTo5Plus3_MSX()
        {
            // MSX: maxBase=8, prefixLength = 8-3 = 5
            var result = _service.Normalize("VERYLONGNAME.TXT", "SJIS", 8, 3);
            Assert.Equal("VERYL001.TXT", result);
        }

        [Fact]
        public void Normalize_LongName_TruncatesTo3Plus3_N88()
        {
            // N88: maxBase=6, prefixLength = 6-3 = 3
            var result = _service.Normalize("VERYLONGNAME.TXT", "X1", 6, 3);
            Assert.Equal("VER001.TXT", result);
        }

        [Fact]
        public void Normalize_Collision_IncrementsCounter()
        {
            // "TESTFILE.TXT" itself doesn't exceed 8.3 limit, but if it conflicts, it should be shortened.
            var existing = new HashSet<string> { "TESTFILE.TXT" };
            // MSX: prefix=5 (TESTF) + 001 = TESTF001
            var result = _service.Normalize("TESTFILE.TXT", "SJIS", 8, 3, existing);
            Assert.Equal("TESTF001.TXT", result);

            // If TESTF001 also exists, it should be TESTF002
            existing.Add("TESTF001.TXT");
            result = _service.Normalize("TESTFILE.TXT", "SJIS", 8, 3, existing);
            Assert.Equal("TESTF002.TXT", result);
        }

        [Fact]
        public void Normalize_NoCollision_ShortName_Unchanged()
        {
            var existing = new HashSet<string> { "OTHER.TXT" };
            var result = _service.Normalize("TEST.TXT", "SJIS", 8, 3, existing);
            Assert.Equal("TEST.TXT", result);
        }

        [Fact]
        public void Normalize_Collision_ShortName_Shortens()
        {
            var existing = new HashSet<string> { "AB.TXT" };
            // prefixLength = 8-3 = 5. but AB is only 2 chars. prefix=AB. Result AB001.TXT
            var result = _service.Normalize("AB.TXT", "SJIS", 8, 3, existing);
            Assert.Equal("AB001.TXT", result);
        }

        [Fact]
        public void Normalize_LongExtension_Truncates()
        {
            var result = _service.Normalize("FILE.LONGEXT", "SJIS", 8, 3);
            Assert.Equal("FILE.LON", result);
        }

        [Fact]
        public void Normalize_NoExtension_Unified_Truncates()
        {
            // Unified (ext=0), max=13 (HuBasic-like) -> prefix=10
            var result = _service.Normalize("VERYLONGFILENAME", "X1", 13, 0);
            Assert.Equal("VERYLONGFI001", result);
        }
    }
}
