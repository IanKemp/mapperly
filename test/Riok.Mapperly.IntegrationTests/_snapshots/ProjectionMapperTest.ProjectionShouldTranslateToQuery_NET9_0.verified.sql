﻿SELECT "o"."CtorValue", "o"."IntValue", "o"."IntInitOnlyValue", "o"."RequiredValue", "o"."StringValue", "o"."RenamedStringValue", "i"."IdValue", "i0"."IdValue", CASE
    WHEN "t"."IntValue" IS NOT NULL THEN "t"."IntValue"
    ELSE 0
END, "t"."IntValue" IS NOT NULL, "t"."IntValue", "t0"."IntValue" IS NOT NULL, "t0"."IntValue", COALESCE("o"."StringNullableTargetNotNullable", ''), "o0"."Id", "o0"."CtorValue", "o0"."DateTimeValueTargetDateOnly", "o0"."DateTimeValueTargetTimeOnly", "o0"."EnumName", "o0"."EnumRawValue", "o0"."EnumReverseStringValue", "o0"."EnumStringValue", "o0"."EnumValue", "o0"."FlatteningIdValue", "o0"."IgnoredIntValue", "o0"."IgnoredStringValue", "o0"."IntInitOnlyValue", "o0"."IntValue", "o0"."ManuallyMapped", "o0"."ManuallyMappedModified", "o0"."NestedNullableIntValue", "o0"."NestedNullableTargetNotNullableIntValue", "o0"."NullableFlatteningIdValue", "o0"."NullableUnflatteningIdValue", "o0"."RecursiveObjectId", "o0"."RenamedStringValue", "o0"."RequiredValue", "o0"."StringNullableTargetNotNullable", "o0"."StringValue", "o0"."SubObjectSubIntValue", "o0"."UnflatteningIdValue", "o"."Id", "i1"."SubIntValue", "t1"."IntValue", "t1"."TestObjectProjectionId", "t2"."IntValue", CAST("o"."EnumValue" AS INTEGER), CAST("o"."EnumName" AS INTEGER), CAST("o"."EnumRawValue" AS INTEGER), CASE "o"."EnumStringValue"
    WHEN 10 THEN 'Value10'
    WHEN 20 THEN 'Value20'
    WHEN 30 THEN 'Value30'
    ELSE CAST("o"."EnumStringValue" AS TEXT)
END, "o"."EnumReverseStringValue", "i1"."SubIntValue" IS NOT NULL, "i1"."BaseIntValue", date("o"."DateTimeValueTargetDateOnly"), "o"."DateTimeValueTargetTimeOnly", "o"."ManuallyMapped", "o"."ManuallyMappedModified" + 10, "t3"."Value", "t3"."Id", "i2"."Id", "i2"."TestObjectProjectionId", "i2"."Value", "l"."Value", "l"."Id"
FROM "Objects" AS "o"
INNER JOIN "IdObject" AS "i" ON "o"."FlatteningIdValue" = "i"."IdValue"
LEFT JOIN "IdObject" AS "i0" ON "o"."NullableFlatteningIdValue" = "i0"."IdValue"
LEFT JOIN "TestObjectNested" AS "t" ON "o"."NestedNullableIntValue" = "t"."IntValue"
LEFT JOIN "TestObjectNested" AS "t0" ON "o"."NestedNullableTargetNotNullableIntValue" = "t0"."IntValue"
LEFT JOIN "Objects" AS "o0" ON "o"."Id" = "o0"."RecursiveObjectId"
LEFT JOIN "InheritanceSubObject" AS "i1" ON "o"."SubObjectSubIntValue" = "i1"."SubIntValue"
LEFT JOIN "TestObjectNested" AS "t1" ON "o"."Id" = "t1"."TestObjectProjectionId"
LEFT JOIN "TestObjectNested" AS "t2" ON "o"."Id" = "t2"."TestObjectProjectionId"
LEFT JOIN "TestObjectProjectionEnumValue" AS "t3" ON "o"."Id" = "t3"."TestObjectProjectionId"
LEFT JOIN "IntegerValue" AS "i2" ON "o"."Id" = "i2"."TestObjectProjectionId"
LEFT JOIN "LongValue" AS "l" ON "o"."Id" = "l"."TestObjectProjectionId"
ORDER BY "o"."Id", "i"."IdValue", "i0"."IdValue", "t"."IntValue", "t0"."IntValue", "o0"."Id", "i1"."SubIntValue", "t1"."IntValue", "t2"."IntValue", "t3"."Id", "i2"."Value", "i2"."Id", "l"."Value"