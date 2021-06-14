using System.Collections.Generic;
using UnityEngine;
using System;

public class BinaryExtension {
    public static uint asBinarySequence(uint[] arr) {
        uint result = 0b0;
        for (int i = 0; i < arr.Length; i++)
            result += arr[i] << (arr.Length - i - 1);
        return result;
    }
}
