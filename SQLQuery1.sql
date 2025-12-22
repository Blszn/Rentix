-- HEDEF KULLANICI
DECLARE @EmailAdresi NVARCHAR(256) = 'rentixAdmin@gmail.com'; 

DECLARE @UserId NVARCHAR(450);
DECLARE @RoleId NVARCHAR(450);

-- 1. 'Admin' rolü yoksa oluştur
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
    PRINT 'Admin rolü oluşturuldu.';
END

-- 2. Kullanıcıyı ve Rolü bul
SELECT @UserId = Id FROM AspNetUsers WHERE Email = @EmailAdresi;
SELECT @RoleId = Id FROM AspNetRoles WHERE Name = 'Admin';

-- 3. Yetkiyi Ver
IF @UserId IS NOT NULL
BEGIN
    -- Zaten yetkisi var mı?
    IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        PRINT 'BAŞARILI: ' + @EmailAdresi + ' artık Admin yetkisine sahip!';
    END
    ELSE
    BEGIN
        PRINT 'BİLGİ: Bu kullanıcı zaten Admin.';
    END
END
ELSE
BEGIN
    PRINT 'HATA: ' + @EmailAdresi + ' bulunamadı! Lütfen önce site üzerinden bu mail ile kayıt olun.';
END