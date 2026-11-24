using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
namespace Vtb.Hr.Web.Services;

public sealed record ExcelTable(string Title, string[] Headers, List<object?[]> Rows);

// A small, dependency-free SpreadsheetML writer. Every string is an inline string, never a formula.
public static class ExcelExport
{
 public const int MaxRows=20000;
 public const string Mime="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
 public static DateTime VietnamNow()=>DateTime.UtcNow.AddHours(7);
 public static void CheckCount(int count){if(count>MaxRows)throw new InvalidDataException("Kết quả vượt 20.000 dòng. Vui lòng thu hẹp bộ lọc để xuất an toàn; hệ thống không cắt bớt dữ liệu.");}
 public static string FileName(string title)=>new string(title.Select(c=>char.IsLetterOrDigit(c)||c=='_'?c:'_').Take(90).ToArray())+"_"+VietnamNow().ToString("yyyyMMdd_HHmmss")+".xlsx";
 public static byte[] Write(ExcelTable table,string scope)
 {
  CheckCount(table.Rows.Count);
  if(table.Headers.Length==0||table.Rows.Any(r=>r.Length!=table.Headers.Length))throw new InvalidDataException("Cấu trúc báo cáo không hợp lệ.");
  using var stream=new MemoryStream();
  using(var zip=new ZipArchive(stream,ZipArchiveMode.Create,true)){
   void Part(string name,string text){using var w=new StreamWriter(zip.CreateEntry(name).Open(),new UTF8Encoding(false));w.Write(text);}
   Part("[Content_Types].xml","""
<?xml version="1.0" encoding="utf-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>
""");
   Part("_rels/.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
   Part("xl/workbook.xml","""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Danh sách" sheetId="1" r:id="rId1"/></sheets></workbook>""");
   Part("xl/_rels/workbook.xml.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
   Part("xl/styles.xml","""
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><numFmts count="1"><numFmt numFmtId="164" formatCode="dd/mm/yyyy"/></numFmts><fonts count="3"><font><sz val="11"/><name val="Calibri"/></font><font><b/><color rgb="FFFFFFFF"/><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="16"/><color rgb="FF16324F"/><name val="Calibri"/></font></fonts><fills count="4"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF16324F"/></patternFill></fill><fill><patternFill patternType="solid"><fgColor rgb="FFF0F5FA"/></patternFill></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="7"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"><alignment vertical="top" wrapText="1"/></xf><xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"><alignment vertical="center" wrapText="1"/></xf><xf numFmtId="0" fontId="2" fillId="0" borderId="0" xfId="0" applyFont="1"/><xf numFmtId="0" fontId="0" fillId="3" borderId="0" xfId="0" applyFill="1"><alignment vertical="top" wrapText="1"/></xf><xf numFmtId="164" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/><xf numFmtId="164" fontId="0" fillId="3" borderId="0" xfId="0" applyNumberFormat="1" applyFill="1"/><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs><cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles></styleSheet>
""");
   const string ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
   using var xml=XmlWriter.Create(zip.CreateEntry("xl/worksheets/sheet1.xml").Open(),new XmlWriterSettings{Encoding=new UTF8Encoding(false),CloseOutput=true});
   void Start(string name)=>xml.WriteStartElement(name,ns);
   void Attr(string name,object value)=>xml.WriteAttributeString(name,Convert.ToString(value,CultureInfo.InvariantCulture));
   void Cell(int row,int col,object? value,int style){Start("c");Attr("r",Column(col)+row);if(value is DateOnly date){style=style==3?5:4;value=date.ToDateTime(TimeOnly.MinValue).ToOADate();}Attr("s",style);
    if(value is int or long or double or decimal){Start("v");xml.WriteString(Convert.ToString(value,CultureInfo.InvariantCulture));xml.WriteEndElement();}
    else {Attr("t","inlineStr");Start("is");Start("t");xml.WriteAttributeString("xml","space",null,"preserve");var text=Convert.ToString(value,CultureInfo.InvariantCulture)??"--";if(text.Length>32767)throw new InvalidDataException("Một ô vượt giới hạn văn bản Excel. Vui lòng thu hẹp bộ lọc; dữ liệu chưa được xuất.");xml.WriteString(new string(text.Where(XmlConvert.IsXmlChar).ToArray()));xml.WriteEndElement();xml.WriteEndElement();}xml.WriteEndElement();}
   void Row(int n,object?[] values,int style){Start("row");Attr("r",n);for(int i=0;i<values.Length;i++)Cell(n,i+1,values[i],style);xml.WriteEndElement();}
   Start("worksheet");Start("sheetViews");Start("sheetView");Attr("workbookViewId",0);Start("pane");Attr("ySplit",5);Attr("topLeftCell","A6");Attr("activePane","bottomLeft");Attr("state","frozen");xml.WriteEndElement();xml.WriteEndElement();xml.WriteEndElement();
   Start("cols");for(int i=0;i<table.Headers.Length;i++){Start("col");Attr("min",i+1);Attr("max",i+1);Attr("width",Math.Clamp(table.Headers[i].Length+6,18,42));Attr("customWidth",1);xml.WriteEndElement();}xml.WriteEndElement();
   Start("sheetData");Row(1,[table.Title],2);Row(2,["Thời gian xuất: "+VietnamNow().ToString("dd/MM/yyyy HH:mm:ss")+" (UTC+7)"],0);Row(3,["Phạm vi: "+scope],0);Row(5,table.Headers.Cast<object?>().ToArray(),1);for(int i=0;i<table.Rows.Count;i++)Row(i+6,table.Rows[i],i%2==0?0:3);xml.WriteEndElement();
   Start("autoFilter");Attr("ref","A5:"+Column(table.Headers.Length)+(table.Rows.Count+5));xml.WriteEndElement();Start("mergeCells");Attr("count",3);for(int i=1;i<=3;i++){Start("mergeCell");Attr("ref","A"+i+":"+Column(table.Headers.Length)+i);xml.WriteEndElement();}xml.WriteEndElement();xml.WriteEndElement();xml.Flush();
  }return stream.ToArray();
 }
 private static string Column(int n){var s="";while(n>0){n--;s=(char)('A'+n%26)+s;n/=26;}return s;}
}
