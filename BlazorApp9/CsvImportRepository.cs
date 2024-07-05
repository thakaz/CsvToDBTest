using System.Reflection;

namespace BlazorApp9.Data
{
    /// <summary>
    /// CSVインポート機能を提供するリポジトリのインターフェース
    /// </summary>
    public interface ICsvImportRepository
    {
        /// <summary>
        /// 指定したエンティティのデータを一新して、新しいデータをぶち込む
        /// </summary>
        /// <param name="entityType">どのエンティティか</param>
        /// <param name="records">新しいデータ</param>
        void ReplaceRecords(Type entityType, IEnumerable<object> records);

        /// <summary>
        /// 既存のデータを気にせず、新しいデータを追加する
        /// </summary>
        /// <param name="modelType">どのモデルか</param>
        /// <param name="records">追加するデータ</param>
        void AddRecordsWithIgnore(Type modelType, IEnumerable<object> records);
    }

    /// <summary>
    /// CSVインポート機能の実装クラス
    /// </summary>
    public class CsvImportRepository : ICsvImportRepository
    {
        private readonly MyDbContext _context;

        /// <summary>
        /// コンストラクタ。DBコンテキストを受け取る
        /// </summary>
        /// <param name="context">DBコンテキスト</param>
        public CsvImportRepository(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// エンティティタイプに応じて、既存のデータを全部消して新しいデータを入れる
        /// </summary>
        /// <param name="entityType">エンティティのタイプ</param>
        /// <param name="records">新しいデータ</param>
        public void ReplaceRecords(Type entityType, IEnumerable<object> records)
        {
            MethodInfo method = this.GetType().GetMethod("ReplaceRecordsGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                                                  ?? throw new InvalidOperationException("ReplaceRecordsGeneric method not found.");
            MethodInfo generic = method.MakeGenericMethod(entityType);
            generic.Invoke(this, [records]);
        }

        /// <summary>
        /// ジェネリック版。既存のデータを全部消して新しいデータを入れる
        /// </summary>
        /// <typeparam name="TEntity">エンティティのタイプ</typeparam>
        /// <param name="records">新しいデータ</param>
        private void ReplaceRecordsGeneric<TEntity>(IEnumerable<TEntity> records) where TEntity : class
        {
            var dbSet = _context.Set<TEntity>();
            dbSet.RemoveRange(dbSet);
            dbSet.AddRange(records);
            _context.SaveChanges();
        }


        /// <summary>
        /// 主キーが被ってたら無視して、新しいデータを追加する
        /// </summary>
        /// <param name="entityType">エンティティのタイプ</param>
        /// <param name="records">追加するデータ</param>
        public void AddRecordsWithIgnore(Type entityType, IEnumerable<object> records)
        {
            MethodInfo method = this.GetType().GetMethod("AddRecordsWithIgnoreGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                                                  ?? throw new InvalidOperationException("AddRecordsWithIgnoreGeneric method not found.");
            MethodInfo generic = method.MakeGenericMethod(entityType);
            generic.Invoke(this, [records]);

        }


        /// <summary>
        /// ジェネリック版。主キーが被ってたらスルーして、新しいデータを追加する
        /// </summary>
        /// <typeparam name="TEntity">エンティティのタイプ</typeparam>
        /// <param name="records">追加するデータ</param>
        private void AddRecordsWithIgnoreGeneric<TEntity>(IEnumerable<TEntity> records) where TEntity : class
        {

            var dbSet = _context.Set<TEntity>();

            //主キー情報
            var keyProperties = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;

            //主キーが重複している場合は無視する
            foreach (var record in records)
            {
                var keyValues = keyProperties.Select(p => record.GetType().GetProperty(p.Name).GetValue(record)).ToArray();
                var exists = dbSet.Find(keyValues) != null;

                if (!exists)
                {
                    dbSet.Add(record);
                }
            }
            _context.SaveChanges();
        }
    }
}
