using System;
using System.Runtime.InteropServices;

namespace EntityWorker.Core.SQLite
{
	public sealed class SQLiteIndex
	{
		private SQLiteIndexInputs inputs;

		private SQLiteIndexOutputs outputs;

		public SQLiteIndexInputs Inputs
		{
			get
			{
				return this.inputs;
			}
		}

		public SQLiteIndexOutputs Outputs
		{
			get
			{
				return this.outputs;
			}
		}

		internal SQLiteIndex(int nConstraint, int nOrderBy)
		{
			this.inputs = new SQLiteIndexInputs(nConstraint, nOrderBy);
			this.outputs = new SQLiteIndexOutputs(nConstraint);
		}

		private static IntPtr AllocateAndInitializeNative(int nConstraint, int nOrderBy)
		{
			int num;
			int num1;
			int num2;
			int num3;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			IntPtr intPtr1 = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			try
			{
				SQLiteIndex.SizeOfNative(out num, out num2, out num1, out num3);
				if (num > 0 && num2 > 0 && num1 > 0 && num3 > 0)
				{
					intPtr = SQLiteMemory.Allocate(num);
					zero1 = SQLiteMemory.Allocate(num2 * nConstraint);
					intPtr1 = SQLiteMemory.Allocate(num1 * nOrderBy);
					zero2 = SQLiteMemory.Allocate(num3 * nConstraint);
					if (intPtr != IntPtr.Zero && zero1 != IntPtr.Zero && intPtr1 != IntPtr.Zero && zero2 != IntPtr.Zero)
					{
						int num4 = 0;
						SQLiteMarshal.WriteInt32(intPtr, num4, nConstraint);
						num4 = SQLiteMarshal.NextOffsetOf(num4, 4, IntPtr.Size);
						SQLiteMarshal.WriteIntPtr(intPtr, num4, zero1);
						num4 = SQLiteMarshal.NextOffsetOf(num4, IntPtr.Size, 4);
						SQLiteMarshal.WriteInt32(intPtr, num4, nOrderBy);
						num4 = SQLiteMarshal.NextOffsetOf(num4, 4, IntPtr.Size);
						SQLiteMarshal.WriteIntPtr(intPtr, num4, intPtr1);
						num4 = SQLiteMarshal.NextOffsetOf(num4, IntPtr.Size, IntPtr.Size);
						SQLiteMarshal.WriteIntPtr(intPtr, num4, zero2);
						zero = intPtr;
					}
				}
			}
			finally
			{
				if (zero == IntPtr.Zero)
				{
					if (zero2 != IntPtr.Zero)
					{
						SQLiteMemory.Free(zero2);
						zero2 = IntPtr.Zero;
					}
					if (intPtr1 != IntPtr.Zero)
					{
						SQLiteMemory.Free(intPtr1);
						intPtr1 = IntPtr.Zero;
					}
					if (zero1 != IntPtr.Zero)
					{
						SQLiteMemory.Free(zero1);
						zero1 = IntPtr.Zero;
					}
					if (intPtr != IntPtr.Zero)
					{
						SQLiteMemory.Free(intPtr);
						intPtr = IntPtr.Zero;
					}
				}
			}
			return zero;
		}

		private static void FreeNative(IntPtr pIndex)
		{
			if (pIndex == IntPtr.Zero)
			{
				return;
			}
			int num = 0;
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			IntPtr zero = SQLiteMarshal.ReadIntPtr(pIndex, num);
			int num1 = num;
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			IntPtr intPtr = SQLiteMarshal.ReadIntPtr(pIndex, num);
			int num2 = num;
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, IntPtr.Size);
			IntPtr zero1 = SQLiteMarshal.ReadIntPtr(pIndex, num);
			int num3 = num;
			if (zero1 != IntPtr.Zero)
			{
				SQLiteMemory.Free(zero1);
				zero1 = IntPtr.Zero;
				SQLiteMarshal.WriteIntPtr(pIndex, num3, zero1);
			}
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
				SQLiteMarshal.WriteIntPtr(pIndex, num2, intPtr);
			}
			if (zero != IntPtr.Zero)
			{
				SQLiteMemory.Free(zero);
				zero = IntPtr.Zero;
				SQLiteMarshal.WriteIntPtr(pIndex, num1, zero);
			}
			if (pIndex != IntPtr.Zero)
			{
				SQLiteMemory.Free(pIndex);
				pIndex = IntPtr.Zero;
			}
		}

		internal static void FromIntPtr(IntPtr pIndex, bool includeOutput, ref SQLiteIndex index)
		{
			if (pIndex == IntPtr.Zero)
			{
				return;
			}
			int num = 0;
			int num1 = SQLiteMarshal.ReadInt32(pIndex, num);
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			IntPtr intPtr = SQLiteMarshal.ReadIntPtr(pIndex, num);
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			int num2 = SQLiteMarshal.ReadInt32(pIndex, num);
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			IntPtr intPtr1 = SQLiteMarshal.ReadIntPtr(pIndex, num);
			IntPtr zero = IntPtr.Zero;
			if (includeOutput)
			{
				num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, IntPtr.Size);
				zero = SQLiteMarshal.ReadIntPtr(pIndex, num);
			}
			index = new SQLiteIndex(num1, num2);
			SQLiteIndexInputs inputs = index.Inputs;
			if (inputs == null)
			{
				return;
			}
			SQLiteIndexConstraint[] constraints = inputs.Constraints;
			if (constraints == null)
			{
				return;
			}
			SQLiteIndexOrderBy[] orderBys = inputs.OrderBys;
			if (orderBys == null)
			{
				return;
			}
			Type type = typeof(UnsafeNativeMethods.sqlite3_index_constraint);
			int num3 = Marshal.SizeOf(type);
			for (int i = 0; i < num1; i++)
			{
				IntPtr intPtr2 = SQLiteMarshal.IntPtrForOffset(intPtr, i * num3);
				UnsafeNativeMethods.sqlite3_index_constraint structure = (UnsafeNativeMethods.sqlite3_index_constraint)Marshal.PtrToStructure(intPtr2, type);
				constraints[i] = new SQLiteIndexConstraint(structure);
			}
			Type type1 = typeof(UnsafeNativeMethods.sqlite3_index_orderby);
			int num4 = Marshal.SizeOf(type1);
			for (int j = 0; j < num2; j++)
			{
				IntPtr intPtr3 = SQLiteMarshal.IntPtrForOffset(intPtr1, j * num4);
				UnsafeNativeMethods.sqlite3_index_orderby sqlite3IndexOrderby = (UnsafeNativeMethods.sqlite3_index_orderby)Marshal.PtrToStructure(intPtr3, type1);
				orderBys[j] = new SQLiteIndexOrderBy(sqlite3IndexOrderby);
			}
			if (includeOutput)
			{
				SQLiteIndexOutputs outputs = index.Outputs;
				if (outputs == null)
				{
					return;
				}
				SQLiteIndexConstraintUsage[] constraintUsages = outputs.ConstraintUsages;
				if (constraintUsages == null)
				{
					return;
				}
				Type type2 = typeof(UnsafeNativeMethods.sqlite3_index_constraint_usage);
				int num5 = Marshal.SizeOf(type2);
				for (int k = 0; k < num1; k++)
				{
					IntPtr intPtr4 = SQLiteMarshal.IntPtrForOffset(zero, k * num5);
					UnsafeNativeMethods.sqlite3_index_constraint_usage sqlite3IndexConstraintUsage = (UnsafeNativeMethods.sqlite3_index_constraint_usage)Marshal.PtrToStructure(intPtr4, type2);
					constraintUsages[k] = new SQLiteIndexConstraintUsage(sqlite3IndexConstraintUsage);
				}
				num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
				outputs.IndexNumber = SQLiteMarshal.ReadInt32(pIndex, num);
				num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
				outputs.IndexString = SQLiteString.StringFromUtf8IntPtr(SQLiteMarshal.ReadIntPtr(pIndex, num));
				num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
				outputs.NeedToFreeIndexString = SQLiteMarshal.ReadInt32(pIndex, num);
				num = SQLiteMarshal.NextOffsetOf(num, 4, 4);
				outputs.OrderByConsumed = SQLiteMarshal.ReadInt32(pIndex, num);
				num = SQLiteMarshal.NextOffsetOf(num, 4, 8);
				outputs.EstimatedCost = new double?(SQLiteMarshal.ReadDouble(pIndex, num));
				num = SQLiteMarshal.NextOffsetOf(num, 8, 8);
				if (outputs.CanUseEstimatedRows())
				{
					outputs.EstimatedRows = new long?(SQLiteMarshal.ReadInt64(pIndex, num));
				}
				num = SQLiteMarshal.NextOffsetOf(num, 8, 4);
				if (outputs.CanUseIndexFlags())
				{
					outputs.IndexFlags = new SQLiteIndexFlags?((SQLiteIndexFlags)SQLiteMarshal.ReadInt32(pIndex, num));
				}
				num = SQLiteMarshal.NextOffsetOf(num, 4, 8);
				if (outputs.CanUseColumnsUsed())
				{
					outputs.ColumnsUsed = new long?(SQLiteMarshal.ReadInt64(pIndex, num));
				}
			}
		}

		private static void SizeOfNative(out int sizeOfInfoType, out int sizeOfConstraintType, out int sizeOfOrderByType, out int sizeOfConstraintUsageType)
		{
			sizeOfInfoType = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_info));
			sizeOfConstraintType = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_constraint));
			sizeOfOrderByType = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_orderby));
			sizeOfConstraintUsageType = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_constraint_usage));
		}

		internal static void ToIntPtr(SQLiteIndex index, IntPtr pIndex, bool includeInput)
		{
			if (index == null)
			{
				return;
			}
			SQLiteIndexOutputs outputs = index.Outputs;
			if (outputs == null)
			{
				return;
			}
			SQLiteIndexConstraintUsage[] constraintUsages = outputs.ConstraintUsages;
			if (constraintUsages == null)
			{
				return;
			}
			SQLiteIndexInputs inputs = null;
			SQLiteIndexConstraint[] constraints = null;
			SQLiteIndexOrderBy[] orderBys = null;
			if (includeInput)
			{
				inputs = index.Inputs;
				if (inputs == null)
				{
					return;
				}
				constraints = inputs.Constraints;
				if (constraints == null)
				{
					return;
				}
				orderBys = inputs.OrderBys;
				if (orderBys == null)
				{
					return;
				}
			}
			if (pIndex == IntPtr.Zero)
			{
				return;
			}
			int num = 0;
			int num1 = SQLiteMarshal.ReadInt32(pIndex, num);
			if (includeInput && num1 != (int)constraints.Length)
			{
				return;
			}
			if (num1 != (int)constraintUsages.Length)
			{
				return;
			}
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			if (includeInput)
			{
				IntPtr intPtr = SQLiteMarshal.ReadIntPtr(pIndex, num);
				int num2 = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_constraint));
				for (int i = 0; i < num1; i++)
				{
					UnsafeNativeMethods.sqlite3_index_constraint sqlite3IndexConstraint = new UnsafeNativeMethods.sqlite3_index_constraint(constraints[i]);
					Marshal.StructureToPtr(sqlite3IndexConstraint, SQLiteMarshal.IntPtrForOffset(intPtr, i * num2), false);
				}
			}
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			int num3 = (includeInput ? SQLiteMarshal.ReadInt32(pIndex, num) : 0);
			if (includeInput && num3 != (int)orderBys.Length)
			{
				return;
			}
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			if (includeInput)
			{
				IntPtr intPtr1 = SQLiteMarshal.ReadIntPtr(pIndex, num);
				int num4 = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_orderby));
				for (int j = 0; j < num3; j++)
				{
					UnsafeNativeMethods.sqlite3_index_orderby sqlite3IndexOrderby = new UnsafeNativeMethods.sqlite3_index_orderby(orderBys[j]);
					Marshal.StructureToPtr(sqlite3IndexOrderby, SQLiteMarshal.IntPtrForOffset(intPtr1, j * num4), false);
				}
			}
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, IntPtr.Size);
			IntPtr intPtr2 = SQLiteMarshal.ReadIntPtr(pIndex, num);
			int num5 = Marshal.SizeOf(typeof(UnsafeNativeMethods.sqlite3_index_constraint_usage));
			for (int k = 0; k < num1; k++)
			{
				UnsafeNativeMethods.sqlite3_index_constraint_usage sqlite3IndexConstraintUsage = new UnsafeNativeMethods.sqlite3_index_constraint_usage(constraintUsages[k]);
				Marshal.StructureToPtr(sqlite3IndexConstraintUsage, SQLiteMarshal.IntPtrForOffset(intPtr2, k * num5), false);
			}
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			SQLiteMarshal.WriteInt32(pIndex, num, outputs.IndexNumber);
			num = SQLiteMarshal.NextOffsetOf(num, 4, IntPtr.Size);
			SQLiteMarshal.WriteIntPtr(pIndex, num, SQLiteString.Utf8IntPtrFromString(outputs.IndexString));
			num = SQLiteMarshal.NextOffsetOf(num, IntPtr.Size, 4);
			SQLiteMarshal.WriteInt32(pIndex, num, (outputs.NeedToFreeIndexString != 0 ? outputs.NeedToFreeIndexString : 1));
			num = SQLiteMarshal.NextOffsetOf(num, 4, 4);
			SQLiteMarshal.WriteInt32(pIndex, num, outputs.OrderByConsumed);
			num = SQLiteMarshal.NextOffsetOf(num, 4, 8);
			if (outputs.EstimatedCost.HasValue)
			{
				SQLiteMarshal.WriteDouble(pIndex, num, outputs.EstimatedCost.GetValueOrDefault());
			}
			num = SQLiteMarshal.NextOffsetOf(num, 8, 8);
			if (outputs.CanUseEstimatedRows() && outputs.EstimatedRows.HasValue)
			{
				SQLiteMarshal.WriteInt64(pIndex, num, outputs.EstimatedRows.GetValueOrDefault());
			}
			num = SQLiteMarshal.NextOffsetOf(num, 8, 4);
			if (outputs.CanUseIndexFlags() && outputs.IndexFlags.HasValue)
			{
				SQLiteMarshal.WriteInt32(pIndex, num, (int)outputs.IndexFlags.GetValueOrDefault());
			}
			num = SQLiteMarshal.NextOffsetOf(num, 4, 8);
			if (outputs.CanUseColumnsUsed() && outputs.ColumnsUsed.HasValue)
			{
				SQLiteMarshal.WriteInt64(pIndex, num, outputs.ColumnsUsed.GetValueOrDefault());
			}
		}
	}
}