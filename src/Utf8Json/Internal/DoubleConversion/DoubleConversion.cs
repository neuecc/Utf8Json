using System;
using System.Collections.Generic;
using System.Text;

namespace Utf8Json.Internal.DoubleConversion
{
    // https://github.com/google/double-conversion/blob/master/double-conversion/fast-dtoa.h
    // https://github.com/google/double-conversion/blob/master/double-conversion/fast-dtoa.cc

    public enum FastDtoaMode
    {
        // Computes the shortest representation of the given input. The returned
        // result will be the most accurate number of this length. Longer
        // representations might be more accurate.
        FAST_DTOA_SHORTEST,
        // Same as FAST_DTOA_SHORTEST but for single-precision floats.
        FAST_DTOA_SHORTEST_SINGLE,
        // Computes a representation where the precision (number of digits) is
        // given as input. The precision is independent of the decimal point.
        FAST_DTOA_PRECISION
    };

    public static class DoubleConversion
    {
        public static unsafe bool FastDtoa(double v,
              FastDtoaMode mode,
              int requested_digits,
              byte* buffer, // Vector<char> buffer
              int* length,
              int* decimal_point)
        {
            bool result = false;
            int decimal_exponent = 0;

            switch (mode)
            {
                case FastDtoaMode.FAST_DTOA_SHORTEST:
                case FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE:
                    result = Grisu3(v, mode, buffer, length, &decimal_exponent);
                    break;
                case FastDtoaMode.FAST_DTOA_PRECISION:
                    result = Grisu3Counted(v, requested_digits,
                                           buffer, length, &decimal_exponent);
                    break;
                default:
                    throw new Exception("unreachable code");
            }
            if (result)
            {
                *decimal_point = *length + decimal_exponent;
                // TODO:unnecesssary
                buffer[*length] = (byte)'\0';
            }
            return result;
        }

        // Provides a decimal representation of v.
        // Returns true if it succeeds, otherwise the result cannot be trusted.
        // There will be *length digits inside the buffer (not null-terminated).
        // If the function returns true then
        //        v == (double) (buffer * 10^decimal_exponent).
        // The digits in the buffer are the shortest representation possible: no
        // 0.09999999999999999 instead of 0.1. The shorter representation will even be
        // chosen even if the longer one would be closer to v.
        // The last digit will be closest to the actual v. That is, even if several
        // digits might correctly yield 'v' when read again, the closest will be
        // computed.
        static unsafe bool Grisu3(double v,
            FastDtoaMode mode,
            byte* buffer, // Vector<char> buffer,
            int* length,
            int* decimal_exponent)
        {
            throw new NotImplementedException();

            //DiyFp w = Double(v).AsNormalizedDiyFp();
            //// boundary_minus and boundary_plus are the boundaries between v and its
            //// closest floating-point neighbors. Any number strictly between
            //// boundary_minus and boundary_plus will round to v when convert to a double.
            //// Grisu3 will never output representations that lie exactly on a boundary.
            //DiyFp boundary_minus, boundary_plus;
            //if (mode == FAST_DTOA_SHORTEST)
            //{
            //    Double(v).NormalizedBoundaries(&boundary_minus, &boundary_plus);
            //}
            //else
            //{
            //    ASSERT(mode == FAST_DTOA_SHORTEST_SINGLE);
            //    float single_v = static_cast<float>(v);
            //    Single(single_v).NormalizedBoundaries(&boundary_minus, &boundary_plus);
            //}
            //ASSERT(boundary_plus.e() == w.e());
            //DiyFp ten_mk;  // Cached power of ten: 10^-k
            //int mk;        // -k
            //int ten_mk_minimal_binary_exponent =
            //   kMinimalTargetExponent - (w.e() + DiyFp::kSignificandSize);
            //int ten_mk_maximal_binary_exponent =
            //   kMaximalTargetExponent - (w.e() + DiyFp::kSignificandSize);
            //PowersOfTenCache::GetCachedPowerForBinaryExponentRange(
            //    ten_mk_minimal_binary_exponent,
            //    ten_mk_maximal_binary_exponent,
            //    &ten_mk, &mk);
            //ASSERT((kMinimalTargetExponent <= w.e() + ten_mk.e() +
            //        DiyFp::kSignificandSize) &&
            //       (kMaximalTargetExponent >= w.e() + ten_mk.e() +
            //        DiyFp::kSignificandSize));
            //// Note that ten_mk is only an approximation of 10^-k. A DiyFp only contains a
            //// 64 bit significand and ten_mk is thus only precise up to 64 bits.

            //// The DiyFp::Times procedure rounds its result, and ten_mk is approximated
            //// too. The variable scaled_w (as well as scaled_boundary_minus/plus) are now
            //// off by a small amount.
            //// In fact: scaled_w - w*10^k < 1ulp (unit in the last place) of scaled_w.
            //// In other words: let f = scaled_w.f() and e = scaled_w.e(), then
            ////           (f-1) * 2^e < w*10^k < (f+1) * 2^e
            //DiyFp scaled_w = DiyFp::Times(w, ten_mk);
            //ASSERT(scaled_w.e() ==
            //       boundary_plus.e() + ten_mk.e() + DiyFp::kSignificandSize);
            //// In theory it would be possible to avoid some recomputations by computing
            //// the difference between w and boundary_minus/plus (a power of 2) and to
            //// compute scaled_boundary_minus/plus by subtracting/adding from
            //// scaled_w. However the code becomes much less readable and the speed
            //// enhancements are not terriffic.
            //DiyFp scaled_boundary_minus = DiyFp::Times(boundary_minus, ten_mk);
            //DiyFp scaled_boundary_plus = DiyFp::Times(boundary_plus, ten_mk);

            //// DigitGen will generate the digits of scaled_w. Therefore we have
            //// v == (double) (scaled_w * 10^-mk).
            //// Set decimal_exponent == -mk and pass it to DigitGen. If scaled_w is not an
            //// integer than it will be updated. For instance if scaled_w == 1.23 then
            //// the buffer will be filled with "123" und the decimal_exponent will be
            //// decreased by 2.
            //int kappa;
            //bool result = DigitGen(scaled_boundary_minus, scaled_w, scaled_boundary_plus,
            //                       buffer, length, &kappa);
            //*decimal_exponent = -mk + kappa;
            //return result;
        }

        // The "counted" version of grisu3 (see above) only generates requested_digits
        // number of digits. This version does not generate the shortest representation,
        // and with enough requested digits 0.1 will at some point print as 0.9999999...
        // Grisu3 is too imprecise for real halfway cases (1.5 will not work) and
        // therefore the rounding strategy for halfway cases is irrelevant.
        static unsafe bool Grisu3Counted(double v,
            int requested_digits,
            byte* buffer, // Vector<char> buffer,
            int* length,
            int* decimal_exponent)
        {
            throw new NotImplementedException();

            //DiyFp w = Double(v).AsNormalizedDiyFp();
            //DiyFp ten_mk;  // Cached power of ten: 10^-k
            //int mk;        // -k
            //int ten_mk_minimal_binary_exponent =
            //   kMinimalTargetExponent - (w.e() + DiyFp::kSignificandSize);
            //int ten_mk_maximal_binary_exponent =
            //   kMaximalTargetExponent - (w.e() + DiyFp::kSignificandSize);
            //PowersOfTenCache::GetCachedPowerForBinaryExponentRange(
            //    ten_mk_minimal_binary_exponent,
            //    ten_mk_maximal_binary_exponent,
            //    &ten_mk, &mk);
            //ASSERT((kMinimalTargetExponent <= w.e() + ten_mk.e() +
            //        DiyFp::kSignificandSize) &&
            //       (kMaximalTargetExponent >= w.e() + ten_mk.e() +
            //        DiyFp::kSignificandSize));
            //// Note that ten_mk is only an approximation of 10^-k. A DiyFp only contains a
            //// 64 bit significand and ten_mk is thus only precise up to 64 bits.

            //// The DiyFp::Times procedure rounds its result, and ten_mk is approximated
            //// too. The variable scaled_w (as well as scaled_boundary_minus/plus) are now
            //// off by a small amount.
            //// In fact: scaled_w - w*10^k < 1ulp (unit in the last place) of scaled_w.
            //// In other words: let f = scaled_w.f() and e = scaled_w.e(), then
            ////           (f-1) * 2^e < w*10^k < (f+1) * 2^e
            //DiyFp scaled_w = DiyFp::Times(w, ten_mk);

            //// We now have (double) (scaled_w * 10^-mk).
            //// DigitGen will generate the first requested_digits digits of scaled_w and
            //// return together with a kappa such that scaled_w ~= buffer * 10^kappa. (It
            //// will not always be exactly the same since DigitGenCounted only produces a
            //// limited number of digits.)
            //int kappa;
            //bool result = DigitGenCounted(scaled_w, requested_digits,
            //                              buffer, length, &kappa);
            //*decimal_exponent = -mk + kappa;
            //return result;
        }

    }
}
