using System;
using Unity.Mathematics;
using UnityEngine;
using Assert = Unity.Assertions.Assert;

namespace Modifier.Runtime.Mathematics
{
    public enum MathGeneratedFunction : ulong
    {
        Float2FloatFloat = 0x0000033400000021UL,
        Float2Float2 = 0x0000004400000021UL,
        Float2Float = 0x0000003400000021UL,
        Float2Bool = 0x0000001400000021UL,
        Float2Int = 0x0000002400000021UL,
        Float3FloatFloatFloat = 0x0000333500000022UL,
        Float3FloatFloat2 = 0x0000043500000022UL,
        Float3Float2Float = 0x0000034500000022UL,
        Float3Float3 = 0x0000005500000022UL,
        Float3Float = 0x0000003500000022UL,
        Float3Bool = 0x0000001500000022UL,
        Float3Int = 0x0000002500000022UL,
        Float4FloatFloatFloatFloat = 0x0003333600000023UL,
        Float4FloatFloatFloat2 = 0x0000433600000023UL,
        Float4FloatFloat2Float = 0x0000343600000023UL,
        Float4FloatFloat3 = 0x0000053600000023UL,
        Float4Float2FloatFloat = 0x0000334600000023UL,
        Float4Float2Float2 = 0x0000044600000023UL,
        Float4Float3Float = 0x0000035600000023UL,
        Float4Float4 = 0x0000006600000023UL,
        Float4Float = 0x0000003600000023UL,
        Float4Bool = 0x0000001600000023UL,
        Float4Int = 0x0000002600000023UL,
        MinIntInt = 0x0000022200000017UL,
        MinFloatFloat = 0x0000033300000017UL,
        MinFloat2Float2 = 0x0000044400000017UL,
        MinFloat3Float3 = 0x0000055500000017UL,
        MinFloat4Float4 = 0x0000066600000017UL,
        MaxIntInt = 0x0000022200000018UL,
        MaxFloatFloat = 0x0000033300000018UL,
        MaxFloat2Float2 = 0x0000044400000018UL,
        MaxFloat3Float3 = 0x0000055500000018UL,
        MaxFloat4Float4 = 0x0000066600000018UL,
        RemapFloatFloatFloatFloatFloat = 0x003333330000001FUL,
        RemapFloat2Float2Float2Float2Float2 = 0x004444440000001FUL,
        RemapFloat3Float3Float3Float3Float3 = 0x005555550000001FUL,
        RemapFloat4Float4Float4Float4Float4 = 0x006666660000001FUL,
        ClampIntIntInt = 0x000022220000001EUL,
        ClampFloatFloatFloat = 0x000033330000001EUL,
        ClampFloat2Float2Float2 = 0x000044440000001EUL,
        ClampFloat3Float3Float3 = 0x000055550000001EUL,
        ClampFloat4Float4Float4 = 0x000066660000001EUL,
        AbsInt = 0x000000220000000EUL,
        AbsFloat = 0x000000330000000EUL,
        AbsFloat2 = 0x000000440000000EUL,
        AbsFloat3 = 0x000000550000000EUL,
        AbsFloat4 = 0x000000660000000EUL,
        DotIntInt = 0x0000022200000014UL,
        DotFloatFloat = 0x0000033300000014UL,
        DotFloat2Float2 = 0x0000044300000014UL,
        DotFloat3Float3 = 0x0000055300000014UL,
        DotFloat4Float4 = 0x0000066300000014UL,
        TanFloat = 0x0000003300000003UL,
        TanFloat2 = 0x0000004400000003UL,
        TanFloat3 = 0x0000005500000003UL,
        TanFloat4 = 0x0000006600000003UL,
        TanhFloat = 0x0000003300000006UL,
        TanhFloat2 = 0x0000004400000006UL,
        TanhFloat3 = 0x0000005500000006UL,
        TanhFloat4 = 0x0000006600000006UL,
        AtanFloat = 0x0000003300000009UL,
        AtanFloat2 = 0x0000004400000009UL,
        AtanFloat3 = 0x0000005500000009UL,
        AtanFloat4 = 0x0000006600000009UL,
        Atan2FloatFloat = 0x000003330000000AUL,
        Atan2Float2Float2 = 0x000004440000000AUL,
        Atan2Float3Float3 = 0x000005550000000AUL,
        Atan2Float4Float4 = 0x000006660000000AUL,
        CosFloat = 0x0000003300000002UL,
        CosFloat2 = 0x0000004400000002UL,
        CosFloat3 = 0x0000005500000002UL,
        CosFloat4 = 0x0000006600000002UL,
        CoshFloat = 0x0000003300000005UL,
        CoshFloat2 = 0x0000004400000005UL,
        CoshFloat3 = 0x0000005500000005UL,
        CoshFloat4 = 0x0000006600000005UL,
        AcosFloat = 0x0000003300000008UL,
        AcosFloat2 = 0x0000004400000008UL,
        AcosFloat3 = 0x0000005500000008UL,
        AcosFloat4 = 0x0000006600000008UL,
        SinFloat = 0x0000003300000001UL,
        SinFloat2 = 0x0000004400000001UL,
        SinFloat3 = 0x0000005500000001UL,
        SinFloat4 = 0x0000006600000001UL,
        SinhFloat = 0x0000003300000004UL,
        SinhFloat2 = 0x0000004400000004UL,
        SinhFloat3 = 0x0000005500000004UL,
        SinhFloat4 = 0x0000006600000004UL,
        AsinFloat = 0x0000003300000007UL,
        AsinFloat2 = 0x0000004400000007UL,
        AsinFloat3 = 0x0000005500000007UL,
        AsinFloat4 = 0x0000006600000007UL,
        FloorFloat = 0x000000330000000DUL,
        FloorFloat2 = 0x000000440000000DUL,
        FloorFloat3 = 0x000000550000000DUL,
        FloorFloat4 = 0x000000660000000DUL,
        CeilFloat = 0x000000330000000CUL,
        CeilFloat2 = 0x000000440000000CUL,
        CeilFloat3 = 0x000000550000000CUL,
        CeilFloat4 = 0x000000660000000CUL,
        RoundFloat = 0x000000330000000BUL,
        RoundFloat2 = 0x000000440000000BUL,
        RoundFloat3 = 0x000000550000000BUL,
        RoundFloat4 = 0x000000660000000BUL,
        SignFloat = 0x0000003300000012UL,
        SignFloat2 = 0x0000004400000012UL,
        SignFloat3 = 0x0000005500000012UL,
        SignFloat4 = 0x0000006600000012UL,
        PowFloatFloat = 0x0000033300000016UL,
        PowFloat2Float2 = 0x0000044400000016UL,
        PowFloat3Float3 = 0x0000055500000016UL,
        PowFloat4Float4 = 0x0000066600000016UL,
        ExpFloat = 0x000000330000000FUL,
        ExpFloat2 = 0x000000440000000FUL,
        ExpFloat3 = 0x000000550000000FUL,
        ExpFloat4 = 0x000000660000000FUL,
        Log2Float = 0x0000003300000011UL,
        Log2Float2 = 0x0000004400000011UL,
        Log2Float3 = 0x0000005500000011UL,
        Log2Float4 = 0x0000006600000011UL,
        Log10Float = 0x0000003300000010UL,
        Log10Float2 = 0x0000004400000010UL,
        Log10Float3 = 0x0000005500000010UL,
        Log10Float4 = 0x0000006600000010UL,
        SqrtFloat = 0x0000003300000013UL,
        SqrtFloat2 = 0x0000004400000013UL,
        SqrtFloat3 = 0x0000005500000013UL,
        SqrtFloat4 = 0x0000006600000013UL,
        NormalizeFloat2 = 0x0000004400000019UL,
        NormalizeFloat3 = 0x0000005500000019UL,
        NormalizeFloat4 = 0x0000006600000019UL,
        NormalizeSafeFloat2Float2 = 0x000004440000001AUL,
        NormalizeSafeFloat3Float3 = 0x000005550000001AUL,
        NormalizeSafeFloat4Float4 = 0x000006660000001AUL,
        LengthFloat = 0x0000003300000020UL,
        LengthFloat2 = 0x0000004300000020UL,
        LengthFloat3 = 0x0000005300000020UL,
        LengthFloat4 = 0x0000006300000020UL,
        DistanceFloatFloat = 0x0000033300000024UL,
        DistanceFloat2Float2 = 0x0000044300000024UL,
        DistanceFloat3Float3 = 0x0000055300000024UL,
        DistanceFloat4Float4 = 0x0000066300000024UL,
        CrossFloat3Float3 = 0x0000055500000015UL,
        ReflectFloat2Float2 = 0x000004440000001CUL,
        ReflectFloat3Float3 = 0x000005550000001CUL,
        ReflectFloat4Float4 = 0x000006660000001CUL,
        RefractFloat2Float2Float = 0x000034440000001DUL,
        RefractFloat3Float3Float = 0x000035550000001DUL,
        RefractFloat4Float4Float = 0x000036660000001DUL,
        AddIntInt = 0x0000022280000001UL,
        AddFloatFloat = 0x0000033380000001UL,
        AddFloat2Float2 = 0x0000044480000001UL,
        AddFloat3Float3 = 0x0000055580000001UL,
        AddFloat4Float4 = 0x0000066680000001UL,
        SubtractIntInt = 0x0000022280000002UL,
        SubtractFloatFloat = 0x0000033380000002UL,
        SubtractFloat2Float2 = 0x0000044480000002UL,
        SubtractFloat3Float3 = 0x0000055580000002UL,
        SubtractFloat4Float4 = 0x0000066680000002UL,
        DivideIntInt = 0x0000022280000004UL,
        DivideFloatFloat = 0x0000033380000004UL,
        MultiplyIntInt = 0x0000022280000003UL,
        MultiplyFloatFloat = 0x0000033380000003UL,
        MultiplyFloat2Float2 = 0x0000044480000003UL,
        MultiplyFloat3Float3 = 0x0000055580000003UL,
        MultiplyFloat4Float4 = 0x0000066680000003UL,
        ModuloIntInt = 0x0000022280000006UL,
        ModuloFloatFloat = 0x0000033380000006UL,
        NegateFloat = 0x0000003380000005UL,
        NegateFloat2 = 0x0000004480000005UL,
        NegateFloat3 = 0x0000005580000005UL,
        NegateFloat4 = 0x0000006680000005UL,
        NegateInt = 0x0000002280000005UL,
        CubicRootFloat = 0x0000003380000007UL,
        NumMathFunctions = 0xFFFFFFFF_FFFFFFFFUL,
    }

    public static class MathGeneratedDelegates
    {
        internal static int GenerationVersion => 1076514401;

        internal static System.Collections.Generic.Dictionary<MathGeneratedFunction, MathValueDelegate> s_Delegates =
            new System.Collections.Generic.Dictionary<MathGeneratedFunction, MathValueDelegate>()
        {
            { MathGeneratedFunction.Float2FloatFloat, (Value[] values) => math.float2(values[0].Float, values[1].Float) },
            { MathGeneratedFunction.Float2Float2, (Value[] values) => math.float2(values[0].Float2) },
            { MathGeneratedFunction.Float2Float, (Value[] values) => math.float2(values[0].Float) },
            { MathGeneratedFunction.Float2Bool, (Value[] values) => math.float2(values[0].Bool) },
            { MathGeneratedFunction.Float2Int, (Value[] values) => math.float2(values[0].Int) },
            { MathGeneratedFunction.Float3FloatFloatFloat, (Value[] values) => math.float3(values[0].Float, values[1].Float, values[2].Float) },
            { MathGeneratedFunction.Float3FloatFloat2, (Value[] values) => math.float3(values[0].Float, values[1].Float2) },
            { MathGeneratedFunction.Float3Float2Float, (Value[] values) => math.float3(values[0].Float2, values[1].Float) },
            { MathGeneratedFunction.Float3Float3, (Value[] values) => math.float3(values[0].Float3) },
            { MathGeneratedFunction.Float3Float, (Value[] values) => math.float3(values[0].Float) },
            { MathGeneratedFunction.Float3Bool, (Value[] values) => math.float3(values[0].Bool) },
            { MathGeneratedFunction.Float3Int, (Value[] values) => math.float3(values[0].Int) },
            { MathGeneratedFunction.Float4FloatFloatFloatFloat, (Value[] values) => math.float4(values[0].Float, values[1].Float, values[2].Float, values[3].Float) },
            { MathGeneratedFunction.Float4FloatFloatFloat2, (Value[] values) => math.float4(values[0].Float, values[1].Float, values[2].Float2) },
            { MathGeneratedFunction.Float4FloatFloat2Float, (Value[] values) => math.float4(values[0].Float, values[1].Float2, values[2].Float) },
            { MathGeneratedFunction.Float4FloatFloat3, (Value[] values) => math.float4(values[0].Float, values[1].Float3) },
            { MathGeneratedFunction.Float4Float2FloatFloat, (Value[] values) => math.float4(values[0].Float2, values[1].Float, values[2].Float) },
            { MathGeneratedFunction.Float4Float2Float2, (Value[] values) => math.float4(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.Float4Float3Float, (Value[] values) => math.float4(values[0].Float3, values[1].Float) },
            { MathGeneratedFunction.Float4Float4, (Value[] values) => math.float4(values[0].Float4) },
            { MathGeneratedFunction.Float4Float, (Value[] values) => math.float4(values[0].Float) },
            { MathGeneratedFunction.Float4Bool, (Value[] values) => math.float4(values[0].Bool) },
            { MathGeneratedFunction.Float4Int, (Value[] values) => math.float4(values[0].Int) },
            { MathGeneratedFunction.MinIntInt, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.min(result.Int, values[i].Int);
              return result;
          } },
            { MathGeneratedFunction.MinFloatFloat, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.min(result.Float, values[i].Float);
              return result;
          } },
            { MathGeneratedFunction.MinFloat2Float2, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.min(result.Float2, values[i].Float2);
              return result;
          } },
            { MathGeneratedFunction.MinFloat3Float3, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.min(result.Float3, values[i].Float3);
              return result;
          } },
            { MathGeneratedFunction.MinFloat4Float4, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.min(result.Float4, values[i].Float4);
              return result;
          } },
            { MathGeneratedFunction.MaxIntInt, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.max(result.Int, values[i].Int);
              return result;
          } },
            { MathGeneratedFunction.MaxFloatFloat, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.max(result.Float, values[i].Float);
              return result;
          } },
            { MathGeneratedFunction.MaxFloat2Float2, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.max(result.Float2, values[i].Float2);
              return result;
          } },
            { MathGeneratedFunction.MaxFloat3Float3, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.max(result.Float3, values[i].Float3);
              return result;
          } },
            { MathGeneratedFunction.MaxFloat4Float4, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = math.max(result.Float4, values[i].Float4);
              return result;
          } },
            { MathGeneratedFunction.RemapFloatFloatFloatFloatFloat, (Value[] values) => math.remap(values[0].Float, values[1].Float, values[2].Float, values[3].Float, values[4].Float) },
            { MathGeneratedFunction.RemapFloat2Float2Float2Float2Float2, (Value[] values) => math.remap(values[0].Float2, values[1].Float2, values[2].Float2, values[3].Float2, values[4].Float2) },
            { MathGeneratedFunction.RemapFloat3Float3Float3Float3Float3, (Value[] values) => math.remap(values[0].Float3, values[1].Float3, values[2].Float3, values[3].Float3, values[4].Float3) },
            { MathGeneratedFunction.RemapFloat4Float4Float4Float4Float4, (Value[] values) => math.remap(values[0].Float4, values[1].Float4, values[2].Float4, values[3].Float4, values[4].Float4) },
            { MathGeneratedFunction.ClampIntIntInt, (Value[] values) => math.clamp(values[0].Int, values[1].Int, values[2].Int) },
            { MathGeneratedFunction.ClampFloatFloatFloat, (Value[] values) => math.clamp(values[0].Float, values[1].Float, values[2].Float) },
            { MathGeneratedFunction.ClampFloat2Float2Float2, (Value[] values) => math.clamp(values[0].Float2, values[1].Float2, values[2].Float2) },
            { MathGeneratedFunction.ClampFloat3Float3Float3, (Value[] values) => math.clamp(values[0].Float3, values[1].Float3, values[2].Float3) },
            { MathGeneratedFunction.ClampFloat4Float4Float4, (Value[] values) => math.clamp(values[0].Float4, values[1].Float4, values[2].Float4) },
            { MathGeneratedFunction.AbsInt, (Value[] values) => math.abs(values[0].Int) },
            { MathGeneratedFunction.AbsFloat, (Value[] values) => math.abs(values[0].Float) },
            { MathGeneratedFunction.AbsFloat2, (Value[] values) => math.abs(values[0].Float2) },
            { MathGeneratedFunction.AbsFloat3, (Value[] values) => math.abs(values[0].Float3) },
            { MathGeneratedFunction.AbsFloat4, (Value[] values) => math.abs(values[0].Float4) },
            { MathGeneratedFunction.DotIntInt, (Value[] values) => math.dot(values[0].Int, values[1].Int) },
            { MathGeneratedFunction.DotFloatFloat, (Value[] values) => math.dot(values[0].Float, values[1].Float) },
            { MathGeneratedFunction.DotFloat2Float2, (Value[] values) => math.dot(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.DotFloat3Float3, (Value[] values) => math.dot(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.DotFloat4Float4, (Value[] values) => math.dot(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.TanFloat, (Value[] values) => math.tan(values[0].Float) },
            { MathGeneratedFunction.TanFloat2, (Value[] values) => math.tan(values[0].Float2) },
            { MathGeneratedFunction.TanFloat3, (Value[] values) => math.tan(values[0].Float3) },
            { MathGeneratedFunction.TanFloat4, (Value[] values) => math.tan(values[0].Float4) },
            { MathGeneratedFunction.TanhFloat, (Value[] values) => math.tanh(values[0].Float) },
            { MathGeneratedFunction.TanhFloat2, (Value[] values) => math.tanh(values[0].Float2) },
            { MathGeneratedFunction.TanhFloat3, (Value[] values) => math.tanh(values[0].Float3) },
            { MathGeneratedFunction.TanhFloat4, (Value[] values) => math.tanh(values[0].Float4) },
            { MathGeneratedFunction.AtanFloat, (Value[] values) => math.atan(values[0].Float) },
            { MathGeneratedFunction.AtanFloat2, (Value[] values) => math.atan(values[0].Float2) },
            { MathGeneratedFunction.AtanFloat3, (Value[] values) => math.atan(values[0].Float3) },
            { MathGeneratedFunction.AtanFloat4, (Value[] values) => math.atan(values[0].Float4) },
            { MathGeneratedFunction.Atan2FloatFloat, (Value[] values) => math.atan2(values[0].Float, values[1].Float) },
            { MathGeneratedFunction.Atan2Float2Float2, (Value[] values) => math.atan2(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.Atan2Float3Float3, (Value[] values) => math.atan2(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.Atan2Float4Float4, (Value[] values) => math.atan2(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.CosFloat, (Value[] values) => math.cos(values[0].Float) },
            { MathGeneratedFunction.CosFloat2, (Value[] values) => math.cos(values[0].Float2) },
            { MathGeneratedFunction.CosFloat3, (Value[] values) => math.cos(values[0].Float3) },
            { MathGeneratedFunction.CosFloat4, (Value[] values) => math.cos(values[0].Float4) },
            { MathGeneratedFunction.CoshFloat, (Value[] values) => math.cosh(values[0].Float) },
            { MathGeneratedFunction.CoshFloat2, (Value[] values) => math.cosh(values[0].Float2) },
            { MathGeneratedFunction.CoshFloat3, (Value[] values) => math.cosh(values[0].Float3) },
            { MathGeneratedFunction.CoshFloat4, (Value[] values) => math.cosh(values[0].Float4) },
            { MathGeneratedFunction.AcosFloat, (Value[] values) => math.acos(values[0].Float) },
            { MathGeneratedFunction.AcosFloat2, (Value[] values) => math.acos(values[0].Float2) },
            { MathGeneratedFunction.AcosFloat3, (Value[] values) => math.acos(values[0].Float3) },
            { MathGeneratedFunction.AcosFloat4, (Value[] values) => math.acos(values[0].Float4) },
            { MathGeneratedFunction.SinFloat, (Value[] values) => math.sin(values[0].Float) },
            { MathGeneratedFunction.SinFloat2, (Value[] values) => math.sin(values[0].Float2) },
            { MathGeneratedFunction.SinFloat3, (Value[] values) => math.sin(values[0].Float3) },
            { MathGeneratedFunction.SinFloat4, (Value[] values) => math.sin(values[0].Float4) },
            { MathGeneratedFunction.SinhFloat, (Value[] values) => math.sinh(values[0].Float) },
            { MathGeneratedFunction.SinhFloat2, (Value[] values) => math.sinh(values[0].Float2) },
            { MathGeneratedFunction.SinhFloat3, (Value[] values) => math.sinh(values[0].Float3) },
            { MathGeneratedFunction.SinhFloat4, (Value[] values) => math.sinh(values[0].Float4) },
            { MathGeneratedFunction.AsinFloat, (Value[] values) => math.asin(values[0].Float) },
            { MathGeneratedFunction.AsinFloat2, (Value[] values) => math.asin(values[0].Float2) },
            { MathGeneratedFunction.AsinFloat3, (Value[] values) => math.asin(values[0].Float3) },
            { MathGeneratedFunction.AsinFloat4, (Value[] values) => math.asin(values[0].Float4) },
            { MathGeneratedFunction.FloorFloat, (Value[] values) => math.floor(values[0].Float) },
            { MathGeneratedFunction.FloorFloat2, (Value[] values) => math.floor(values[0].Float2) },
            { MathGeneratedFunction.FloorFloat3, (Value[] values) => math.floor(values[0].Float3) },
            { MathGeneratedFunction.FloorFloat4, (Value[] values) => math.floor(values[0].Float4) },
            { MathGeneratedFunction.CeilFloat, (Value[] values) => math.ceil(values[0].Float) },
            { MathGeneratedFunction.CeilFloat2, (Value[] values) => math.ceil(values[0].Float2) },
            { MathGeneratedFunction.CeilFloat3, (Value[] values) => math.ceil(values[0].Float3) },
            { MathGeneratedFunction.CeilFloat4, (Value[] values) => math.ceil(values[0].Float4) },
            { MathGeneratedFunction.RoundFloat, (Value[] values) => math.round(values[0].Float) },
            { MathGeneratedFunction.RoundFloat2, (Value[] values) => math.round(values[0].Float2) },
            { MathGeneratedFunction.RoundFloat3, (Value[] values) => math.round(values[0].Float3) },
            { MathGeneratedFunction.RoundFloat4, (Value[] values) => math.round(values[0].Float4) },
            { MathGeneratedFunction.SignFloat, (Value[] values) => math.sign(values[0].Float) },
            { MathGeneratedFunction.SignFloat2, (Value[] values) => math.sign(values[0].Float2) },
            { MathGeneratedFunction.SignFloat3, (Value[] values) => math.sign(values[0].Float3) },
            { MathGeneratedFunction.SignFloat4, (Value[] values) => math.sign(values[0].Float4) },
            { MathGeneratedFunction.PowFloatFloat, (Value[] values) => math.pow(values[0].Float, values[1].Float) },
            { MathGeneratedFunction.PowFloat2Float2, (Value[] values) => math.pow(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.PowFloat3Float3, (Value[] values) => math.pow(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.PowFloat4Float4, (Value[] values) => math.pow(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.ExpFloat, (Value[] values) => math.exp(values[0].Float) },
            { MathGeneratedFunction.ExpFloat2, (Value[] values) => math.exp(values[0].Float2) },
            { MathGeneratedFunction.ExpFloat3, (Value[] values) => math.exp(values[0].Float3) },
            { MathGeneratedFunction.ExpFloat4, (Value[] values) => math.exp(values[0].Float4) },
            { MathGeneratedFunction.Log2Float, (Value[] values) => math.log2(values[0].Float) },
            { MathGeneratedFunction.Log2Float2, (Value[] values) => math.log2(values[0].Float2) },
            { MathGeneratedFunction.Log2Float3, (Value[] values) => math.log2(values[0].Float3) },
            { MathGeneratedFunction.Log2Float4, (Value[] values) => math.log2(values[0].Float4) },
            { MathGeneratedFunction.Log10Float, (Value[] values) => math.log10(values[0].Float) },
            { MathGeneratedFunction.Log10Float2, (Value[] values) => math.log10(values[0].Float2) },
            { MathGeneratedFunction.Log10Float3, (Value[] values) => math.log10(values[0].Float3) },
            { MathGeneratedFunction.Log10Float4, (Value[] values) => math.log10(values[0].Float4) },
            { MathGeneratedFunction.SqrtFloat, (Value[] values) => math.sqrt(values[0].Float) },
            { MathGeneratedFunction.SqrtFloat2, (Value[] values) => math.sqrt(values[0].Float2) },
            { MathGeneratedFunction.SqrtFloat3, (Value[] values) => math.sqrt(values[0].Float3) },
            { MathGeneratedFunction.SqrtFloat4, (Value[] values) => math.sqrt(values[0].Float4) },
            { MathGeneratedFunction.NormalizeFloat2, (Value[] values) => math.normalize(values[0].Float2) },
            { MathGeneratedFunction.NormalizeFloat3, (Value[] values) => math.normalize(values[0].Float3) },
            { MathGeneratedFunction.NormalizeFloat4, (Value[] values) => math.normalize(values[0].Float4) },
            { MathGeneratedFunction.NormalizeSafeFloat2Float2, (Value[] values) => math.normalizesafe(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.NormalizeSafeFloat3Float3, (Value[] values) => math.normalizesafe(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.NormalizeSafeFloat4Float4, (Value[] values) => math.normalizesafe(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.LengthFloat, (Value[] values) => math.length(values[0].Float) },
            { MathGeneratedFunction.LengthFloat2, (Value[] values) => math.length(values[0].Float2) },
            { MathGeneratedFunction.LengthFloat3, (Value[] values) => math.length(values[0].Float3) },
            { MathGeneratedFunction.LengthFloat4, (Value[] values) => math.length(values[0].Float4) },
            { MathGeneratedFunction.DistanceFloatFloat, (Value[] values) => math.distance(values[0].Float, values[1].Float) },
            { MathGeneratedFunction.DistanceFloat2Float2, (Value[] values) => math.distance(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.DistanceFloat3Float3, (Value[] values) => math.distance(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.DistanceFloat4Float4, (Value[] values) => math.distance(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.CrossFloat3Float3, (Value[] values) => math.cross(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.ReflectFloat2Float2, (Value[] values) => math.reflect(values[0].Float2, values[1].Float2) },
            { MathGeneratedFunction.ReflectFloat3Float3, (Value[] values) => math.reflect(values[0].Float3, values[1].Float3) },
            { MathGeneratedFunction.ReflectFloat4Float4, (Value[] values) => math.reflect(values[0].Float4, values[1].Float4) },
            { MathGeneratedFunction.RefractFloat2Float2Float, (Value[] values) => math.refract(values[0].Float2, values[1].Float2, values[2].Float) },
            { MathGeneratedFunction.RefractFloat3Float3Float, (Value[] values) => math.refract(values[0].Float3, values[1].Float3, values[2].Float) },
            { MathGeneratedFunction.RefractFloat4Float4Float, (Value[] values) => math.refract(values[0].Float4, values[1].Float4, values[2].Float) },
            { MathGeneratedFunction.AddIntInt, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Int + values[i].Int;
              return result;
          } },
            { MathGeneratedFunction.AddFloatFloat, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float + values[i].Float;
              return result;
          } },
            { MathGeneratedFunction.AddFloat2Float2, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float2 + values[i].Float2;
              return result;
          } },
            { MathGeneratedFunction.AddFloat3Float3, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float3 + values[i].Float3;
              return result;
          } },
            { MathGeneratedFunction.AddFloat4Float4, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float4 + values[i].Float4;
              return result;
          } },
            { MathGeneratedFunction.SubtractIntInt, (Value[] values) => values[0].Int - values[1].Int },
            { MathGeneratedFunction.SubtractFloatFloat, (Value[] values) => values[0].Float - values[1].Float },
            { MathGeneratedFunction.SubtractFloat2Float2, (Value[] values) => values[0].Float2 - values[1].Float2 },
            { MathGeneratedFunction.SubtractFloat3Float3, (Value[] values) => values[0].Float3 - values[1].Float3 },
            { MathGeneratedFunction.SubtractFloat4Float4, (Value[] values) => values[0].Float4 - values[1].Float4 },
            { MathGeneratedFunction.DivideIntInt, (Value[] values) => values[0].Int / values[1].Int },
            { MathGeneratedFunction.DivideFloatFloat, (Value[] values) => values[0].Float / values[1].Float },
            { MathGeneratedFunction.MultiplyIntInt, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Int * values[i].Int;
              return result;
          } },
            { MathGeneratedFunction.MultiplyFloatFloat, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float * values[i].Float;
              return result;
          } },
            { MathGeneratedFunction.MultiplyFloat2Float2, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float2 * values[i].Float2;
              return result;
          } },
            { MathGeneratedFunction.MultiplyFloat3Float3, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float3 * values[i].Float3;
              return result;
          } },
            { MathGeneratedFunction.MultiplyFloat4Float4, (Value[] values) =>
          {
              Assert.IsTrue(values.Length >= 2);
              var result = values[0];
              for (int i = 1; i < values.Length; ++i)
                  result = result.Float4 * values[i].Float4;
              return result;
          } },
            { MathGeneratedFunction.ModuloIntInt, (Value[] values) => values[0].Int % values[1].Int },
            { MathGeneratedFunction.ModuloFloatFloat, (Value[] values) => values[0].Float % values[1].Float },
            { MathGeneratedFunction.NegateFloat, (Value[] values) => - values[0].Float },
            { MathGeneratedFunction.NegateFloat2, (Value[] values) => - values[0].Float2 },
            { MathGeneratedFunction.NegateFloat3, (Value[] values) => - values[0].Float3 },
            { MathGeneratedFunction.NegateFloat4, (Value[] values) => - values[0].Float4 },
            { MathGeneratedFunction.NegateInt, (Value[] values) => - values[0].Int },
            { MathGeneratedFunction.CubicRootFloat, (Value[] values) => math.pow(math.abs(values[0].Float), 1f / 3f) },
        };
    }
}
