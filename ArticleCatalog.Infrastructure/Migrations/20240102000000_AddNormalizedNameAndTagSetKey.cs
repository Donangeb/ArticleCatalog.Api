using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticleCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedNameAndTagSetKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавляем колонку NormalizedName в tags
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "tags",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            // Заполняем NormalizedName для существующих записей
            migrationBuilder.Sql("UPDATE tags SET \"NormalizedName\" = LOWER(TRIM(\"Name\"));");

            // Удаляем старый индекс на Name
            migrationBuilder.DropIndex(
                name: "IX_tags_Name",
                table: "tags");

            // Создаем новый уникальный индекс на NormalizedName
            migrationBuilder.CreateIndex(
                name: "IX_tags_NormalizedName",
                table: "tags",
                column: "NormalizedName",
                unique: true);

            // Добавляем колонку TagSetKey в articles
            migrationBuilder.AddColumn<string>(
                name: "TagSetKey",
                table: "articles",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            // Вычисляем TagSetKey для существующих статей
            // Это сложный запрос, который создает TagSetKey из связанных тегов
            migrationBuilder.Sql(@"
                UPDATE articles a
                SET ""TagSetKey"" = (
                    SELECT STRING_AGG(LOWER(TRIM(t.""Name"")), '|' ORDER BY LOWER(TRIM(t.""Name"")))
                    FROM article_tags at
                    INNER JOIN tags t ON at.""TagId"" = t.""Id""
                    WHERE at.""ArticleId"" = a.""Id""
                )
                WHERE EXISTS (
                    SELECT 1 FROM article_tags WHERE ""ArticleId"" = a.""Id""
                );
            ");

            // Создаем индекс для оптимизации поиска по TagSetKey
            migrationBuilder.CreateIndex(
                name: "IX_articles_TagSetKey",
                table: "articles",
                column: "TagSetKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаляем индекс на TagSetKey
            migrationBuilder.DropIndex(
                name: "IX_articles_TagSetKey",
                table: "articles");

            // Удаляем колонку TagSetKey
            migrationBuilder.DropColumn(
                name: "TagSetKey",
                table: "articles");

            // Удаляем индекс на NormalizedName
            migrationBuilder.DropIndex(
                name: "IX_tags_NormalizedName",
                table: "tags");

            // Восстанавливаем старый индекс на Name
            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                table: "tags",
                column: "Name",
                unique: true);

            // Удаляем колонку NormalizedName
            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "tags");
        }
    }
}

