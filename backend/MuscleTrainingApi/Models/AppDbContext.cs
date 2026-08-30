using Microsoft.EntityFrameworkCore;

namespace MuscleTrainingApi.Models;

public class AppDbContext : DbContext{
    public AppDbContext( DbContextOptions<AppDbContext>options ) : base( options ){

    }

    public DbSet<Workout> Workout { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Cardio> Cardio { get; set; } = null!;

    protected override void OnModelCreating( ModelBuilder modelBuilder ){
        // 登録時のSQL(AuthController)は "CreatedAt" を指定していないため、
        // DB側の既定値で現在日時が入る必要がある。これを書かないと NOT NULL 違反になる。
        modelBuilder.Entity<User>()
            .Property( u => u.CreatedAt )
            .HasDefaultValueSql( "now()" );

        // --- 所有者(Users)への外部キー ---
        // Cascade: 退会したら、その人の部位・種目・記録をまとめて削除する
        modelBuilder.Entity<Category>()
            .HasOne<User>().WithMany()
            .HasForeignKey( c => c.User_Id )
            .OnDelete( DeleteBehavior.Cascade );

        modelBuilder.Entity<Exercise>()
            .HasOne<User>().WithMany()
            .HasForeignKey( e => e.User_Id )
            .OnDelete( DeleteBehavior.Cascade );

        modelBuilder.Entity<Workout>()
            .HasOne<User>().WithMany()
            .HasForeignKey( w => w.User_Id )
            .OnDelete( DeleteBehavior.Cascade );

        // --- データ同士の外部キー ---
        // NoAction: 種目が残っている部位、記録が残っている種目は削除できない。
        // Restrict ではなく NoAction を使うのは、退会時の一括削除を成立させるため。
        // Restrict は「その瞬間」に違反を判定するので、同じ削除処理の中で
        // 子テーブルも消える予定でもエラーになってしまう。
        // NoAction は文の終わりに判定するため、両方消える場合は正しく通る。
        modelBuilder.Entity<Exercise>()
            .HasOne<Category>().WithMany()
            .HasForeignKey( e => e.Category_Id )
            .OnDelete( DeleteBehavior.NoAction );

        modelBuilder.Entity<Workout>()
            .HasOne<Exercise>().WithMany()
            .HasForeignKey( w => w.Exercise_Id )
            .OnDelete( DeleteBehavior.NoAction );

        // --- 一意制約：同じ人の中での名前の重複を禁止する ---
        // 他人が同じ名前を使うのは自由なので、User_Id との複合にする
        modelBuilder.Entity<Category>()
            .HasIndex( c => new { c.User_Id, c.Category_Name } )
            .IsUnique()
            .HasDatabaseName( "UQ_Categories_User_Name" );

        modelBuilder.Entity<Exercise>()
            .HasIndex( e => new { e.User_Id, e.Category_Id, e.Exercise_Name } )
            .IsUnique()
            .HasDatabaseName( "UQ_Exercises_User_Cat_Name" );

        // --- 検索用インデックス：履歴一覧の絞り込みと並び替えを速くする ---
        modelBuilder.Entity<Workout>()
            .HasIndex( w => new { w.User_Id, w.CreatedAt } )
            .HasDatabaseName( "IX_Workout_User_CreatedAt" );

        // --- 有酸素運動 ---
        // 種目マスタを持たない設計のため、Users への外部キーだけを持つ
        modelBuilder.Entity<Cardio>()
            .HasOne<User>().WithMany()
            .HasForeignKey( c => c.User_Id )
            .OnDelete( DeleteBehavior.Cascade );

        modelBuilder.Entity<Cardio>()
            .HasIndex( c => new { c.User_Id, c.CreatedAt } )
            .HasDatabaseName( "IX_Cardio_User_CreatedAt" );

        // 保存時のSQLが "CreatedAt" に NOW() を使うため、DB側の既定値も揃えておく
        modelBuilder.Entity<Cardio>()
            .Property( c => c.CreatedAt )
            .HasDefaultValueSql( "now()" );
    }
}