﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B? Map(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        if (source.Value != null)
        {
            target.Value = global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source.Value, x => MapToD(x)));
        }
        else
        {
            target.Value = null;
        }
        return target;
    }

    private global::D? MapToD(global::C? source)
    {
        if (source == null)
            return default;
        var target = new global::D();
        target.Value = source.Value;
        return target;
    }
}