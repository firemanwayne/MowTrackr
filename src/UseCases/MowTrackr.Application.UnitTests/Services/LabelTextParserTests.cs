using MowTrackr.Application.Services;

namespace MowTrackr.Application.UnitTests.Services;

public sealed class LabelTextParserTests
{
    [Fact]
    public void Parse_SeparateLabelledLines_ReadsAllThree()
    {
        var reading = LabelTextParser.Parse("TORO\nMODEL NO. 74591\nSERIAL NO. 408123456\nMade in USA");

        Assert.Equal("Toro", reading.Manufacturer);

        Assert.Equal("74591", reading.Model);

        Assert.Equal("408123456", reading.SerialNumber);
    }

    [Fact]
    public void Parse_ModelAndSerialOnOneLine_ReadsBoth()
    {
        var reading = LabelTextParser.Parse("HUSQVARNA\nMODEL NO. Z254F SERIAL NO. 0098123");

        Assert.Equal("Husqvarna", reading.Manufacturer);

        Assert.Equal("Z254F", reading.Model);

        Assert.Equal("0098123", reading.SerialNumber);
    }

    [Theory]
    [InlineData("S/N: HRX-1234567", "HRX-1234567")]
    [InlineData("S/N 8827341", "8827341")]
    [InlineData("SER. NO. A1B2C3D4", "A1B2C3D4")]
    [InlineData("Serial Number: 30045178", "30045178")]
    [InlineData("SN#445566778", "445566778")]
    public void Parse_CommonSerialLabellings_AreRecognized(string line, string expected)
    {
        Assert.Equal(expected, LabelTextParser.Parse(line).SerialNumber);
    }

    [Fact]
    public void Parse_ValueOnTheLineBelowTheKeyword_IsStillRead()
    {
        var reading = LabelTextParser.Parse("MODEL\n22318\nSERIAL NO.\n314159265");

        Assert.Equal("22318", reading.Model);

        Assert.Equal("314159265", reading.SerialNumber);
    }

    [Fact]
    public void Parse_MultiWordBrand_IsFound()
    {
        Assert.Equal("John Deere", LabelTextParser.Parse("JOHN DEERE\nMODEL: X350\nS/N 1GX350A123456").Manufacturer);

        Assert.Equal("Briggs & Stratton", LabelTextParser.Parse("Briggs & Stratton Corp.\nModel 12V334").Manufacturer);
    }

    [Fact]
    public void Parse_BrandInsideAnotherWord_IsNotFound()
    {
        Assert.Null(LabelTextParser.Parse("CARGO CAPACITY 500 LBS").Manufacturer);

        Assert.Null(LabelTextParser.Parse("TORONTO ONTARIO").Manufacturer);
    }

    [Fact]
    public void Parse_SerialWithoutADigit_IsRejected()
    {
        Assert.Null(LabelTextParser.Parse("SERIAL NO. UNKNOWN").SerialNumber);
    }

    [Fact]
    public void Parse_KeywordsWithNoValue_GiveNothing()
    {
        var reading = LabelTextParser.Parse("MODEL NO.\nSERIAL NO.");

        Assert.Null(reading.Model);

        Assert.Null(reading.SerialNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \n \n ")]
    [InlineData("Keep hands and feet away. Read the operator's manual before use.")]
    public void Parse_NoUsefulText_IsEmpty(string? text)
    {
        Assert.True(LabelTextParser.Parse(text).IsEmpty);
    }

    [Fact]
    public void Parse_MessyOcrSpacing_IsTolerated()
    {
        var reading = LabelTextParser.Parse("  EXMARK   \r\n  MODEL    NO :   LZS740  \r\n  SERIAL   NO :  316 \r\n  S/N  ::  412233445 ");

        Assert.Equal("Exmark", reading.Manufacturer);

        Assert.Equal("LZS740", reading.Model);

        Assert.Equal("412233445", reading.SerialNumber);
    }

    [Fact]
    public void Parse_JohnDeereProductIdentificationPlate_ReadsPinAndModel()
    {
        var reading = LabelTextParser.Parse("JOHN DEERE\nProduct Identification Number\n*1LV4066RLPN415746*\nTRACTOR 4066R\nASSEMBLED IN USA\nDEERE & COMPANY, MOLINE, ILLINOIS, USA");

        Assert.Equal("John Deere", reading.Manufacturer);

        Assert.Equal("4066R", reading.Model);

        Assert.Equal("1LV4066RLPN415746", reading.SerialNumber);
    }

    [Fact]
    public void Parse_PinWithoutItsHeading_IsFoundByShape()
    {
        Assert.Equal("1LV4066RLPN415746", LabelTextParser.Parse("JOHN DEERE\n* 1LV4066RLPN415746 *\nTRACTOR 4066R").SerialNumber);
    }

    [Fact]
    public void Parse_EngineSerialPlate_UsesTheEngineSerial()
    {
        var reading = LabelTextParser.Parse("JOHN DEERE\nEngine Serial Number\n*PE6068U123456*\n6068HFC09\nDEERE & COMPANY MOLINE, ILLINOIS");

        Assert.Equal("PE6068U123456", reading.SerialNumber);
    }

    [Fact]
    public void Parse_EngineSerialAndMachineSerial_PrefersTheMachine()
    {
        var reading = LabelTextParser.Parse("Engine Serial No. PE6068U123456\nSerial No. 1TC0Z254ABC001234");

        Assert.Equal("1TC0Z254ABC001234", reading.SerialNumber);
    }

    [Fact]
    public void Parse_RealOcrOfJohnDeerePinPlate_ReadsAllThreeFields()
    {
        const string ocr = "JOHN DEERE ; | Jr | his TRACTOR 4066R 18 | : DEERE & COMPANY, NOLWE, LOS, USA : | JOHN DEERE ~~ | Product Identification Number : / | . *1LV4066RLPN415746 + _ | oni iia ?] | TRACTOR 4066R |e | ASSEMBLED N USA. RB | | DEERE & COMPANY, NOLIE, ALS, 84 Ww -";

        var reading = LabelTextParser.Parse(ocr.Replace(" | ", "\n"));

        Assert.Equal("John Deere", reading.Manufacturer);

        Assert.Equal("4066R", reading.Model);

        Assert.Equal("1LV4066RLPN415746", reading.SerialNumber);
    }

    [Fact]
    public void Parse_StrayLetterAfterTheKeyword_FallsBackToTheNextLine()
    {
        Assert.Equal("1LV4066RLPN415746", LabelTextParser.Parse("Product Identification Number a\n+ 1LV4066RLPN415746 *").SerialNumber);
    }

    [Fact]
    public void Parse_RealOcrOfJohnDeereEngineSerialPlate_JoinsTheSplitSerial()
    {
        const string ocr = "JOHN DEERE | X: Engine Serial Number 1 | 9» ii] | 3 6068HFC09 i | ~ DEERE & COMPANY MOLINE, ILLINOIS Made In Mexico | JOHN DEERE | Engine Serial Number | ® : PE6068U1 23456» ® | NTE RTO ET ERS | DEERE & OMPANY MOLINE, ILLINOIS Made In Mexico | JOHN DEERE ~~ ® | Engine Serjal Number k | ’ Eo + PE6068U 123456 » ®";

        var reading = LabelTextParser.Parse(ocr.Replace(" | ", "\n"));

        Assert.Equal("John Deere", reading.Manufacturer);

        Assert.Equal("PE6068U123456", reading.SerialNumber);
    }

    [Theory]
    [InlineData("SERIAL NO. PE6068U 123456", "PE6068U123456")]
    [InlineData("SERIAL NO. 1LV4066R LPN415746", "1LV4066RLPN415746")]
    [InlineData("SERIAL NO. 1234567 2019", "1234567")]
    [InlineData("SERIAL NO. 408123456 MADE IN USA", "408123456")]
    public void Parse_SerialSplitBySpaces_JoinsOnlyRealFragments(string text, string expected)
    {
        Assert.Equal(expected, LabelTextParser.Parse(text).SerialNumber);
    }
}
