using System;

namespace WorkingTools.Static
{
    /// <summary>
    /// ��������� ����� ��� ������ ������� api � ����������� �� ������
    /// </summary>
    public static class caller
    {
        /// <summary>
        /// ����� ������ api � ����������� �� ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="version">������</param>
        /// <param name="funcs">����� �������</param>
        /// <returns></returns>
        public static T Call<T>(int version, params Func<T>[] funcs)
        {
            if (funcs == null) throw new ArgumentNullException(nameof(funcs));
            if (funcs.Length <= 0) throw new ArgumentOutOfRangeException(nameof(funcs), @"����������� �����������");
            if (version <= 0) throw new Exception("�� ������� ������ ������");
            version--;
            if (funcs.Length < version) throw new ArgumentOutOfRangeException(nameof(funcs), $@"����������� ����������� ���������� ��� ������ {version}");

            return funcs[version]();
        }
    }
}