UPDATE "Videos" SET "Status"=1,"PublishedAt"=NOW()-INTERVAL'10 days' WHERE "Id"='a27f389d-7ca4-48f2-aa0a-fd099f89fe5d';
UPDATE "Videos" SET "Status"=1,"PublishedAt"=NOW()-INTERVAL'1 day'  WHERE "Id"='194b9742-961f-4bf7-ad80-46ade06c6a18';
UPDATE "Videos" SET "Status"=1,"PublishedAt"=NOW()-INTERVAL'5 days' WHERE "Id"='92180fc9-a812-4687-9fbf-1431c71e1256';
SELECT "Id","Status","PublishedAt","CreatedAt" FROM "Videos" WHERE "Id" IN ('a27f389d-7ca4-48f2-aa0a-fd099f89fe5d','194b9742-961f-4bf7-ad80-46ade06c6a18','92180fc9-a812-4687-9fbf-1431c71e1256') ORDER BY "PublishedAt" DESC;
