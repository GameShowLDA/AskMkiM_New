namespace UI.Controls.Archive.Models
{
  /// <summary>
  /// OPK-файл.
  /// </summary>
  public class OpkModel
  {
    /// <summary>
    /// Обозначение ОК.
    /// </summary>
    public string Marking { get; set; }

    /// <summary>
    /// Наименование ОК.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Заказ.
    /// </summary>
    public string Order { get; set; }

    /// <summary>
    /// Файл OPK.
    /// </summary>
    public string OpkFilename { get; set; }

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime Creation { get; set; }

    /// <summary>
    /// Цех.
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// Примечания.
    /// </summary>
    public string Description { get; set; }

    public OpkModel(string marking, string name, string order, string opkFilename, DateTime creation, string department, string description)
    {
      Marking = marking;
      Name = name;
      Order = order;
      OpkFilename = opkFilename;
      Creation = creation;
      Department = department;
      Description = description;
    }

    public OpkModel() { }
  }
}
