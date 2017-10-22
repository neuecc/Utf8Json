﻿using System;
using Xunit;

namespace Utf8Json.Tests
{
    public class MultiDimentionalArrayTest
    {
        T Convert<T>(T value)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
        }

        [Theory]
        [InlineData(100, 100, 10, 5)]
        [InlineData(10, 20, 15, 5)]
        [InlineData(3, 5, 10, 15)]
        [InlineData(3, 5, 10, 15)]
        public void MultiDimentional(int dataI, int dataJ, int dataK, int dataL)
        {
            var two = new ValueTuple<int, int>[dataI, dataJ];
            var three = new ValueTuple<int, int, int>[dataI, dataJ, dataK];
            var four = new ValueTuple<int, int, int, int>[dataI, dataJ, dataK, dataL];

            for (var i = 0; i < dataI; i++)
            {
                for (var j = 0; j < dataJ; j++)
                {
                    two[i, j] = (i, j);
                    for (var k = 0; k < dataK; k++)
                    {
                        three[i, j, k] = (i, j, k);
                        for (var l = 0; l < dataL; l++)
                        {
                            four[i, j, k, l] = (i, j, k, l);
                        }
                    }
                }
            }

            var cTwo = Convert(two);
            var cThree = Convert(three);
            var cFour = Convert(four);

            cTwo.Length.Is(two.Length);
            cThree.Length.Is(three.Length);
            cFour.Length.Is(four.Length);

            for (var i = 0; i < dataI; i++)
            {
                for (var j = 0; j < dataJ; j++)
                {
                    cTwo[i, j].Is(two[i, j]);
                    for (var k = 0; k < dataK; k++)
                    {
                        cThree[i, j, k].Is(three[i, j, k]);
                        for (var l = 0; l < dataL; l++)
                        {
                            cFour[i, j, k, l].Is(four[i, j, k, l]);
                        }
                    }
                }
            }
        }
    }
}
