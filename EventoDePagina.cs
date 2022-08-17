using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GeradorDeRelatoriosEmPdf
{
    class EventoDePagina : PdfPageEventHelper
    {
        private PdfContentByte wdc;

        private BaseFont fonteBaseRodape { get; set; }
        private iTextSharp.text.Font fonteRodape { get; set; }
        public int totalPaginas { get; set; }

        public EventoDePagina(int totalPaginas)
        {
            fonteBaseRodape = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            fonteRodape = new iTextSharp.text.Font(fonteBaseRodape, 8f, iTextSharp.text.Font.NORMAL, BaseColor.Black);
            this.totalPaginas = totalPaginas;
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            base.OnOpenDocument(writer, document);
            this.wdc = writer.DirectContent;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            AdicionarMomentoGeracaoRelatorio(writer, document);
            AdicionarNumeroDasPaginas(writer, document);

        }

        private void AdicionarMomentoGeracaoRelatorio(PdfWriter writer, Document document)
        {
            var textoMomentoGeração = $"Gerado em {DateTime.Now.ToShortDateString()} as " + $"{DateTime.Now.ToShortTimeString()}";

            wdc.BeginText();
            wdc.SetFontAndSize(fonteRodape.BaseFont, fonteRodape.Size);
            wdc.SetTextMatrix(document.LeftMargin, document.BottomMargin * 0.75f);
            wdc.ShowText(textoMomentoGeração);
            wdc.EndText();
        }
        private void AdicionarNumeroDasPaginas(PdfWriter writer, Document document)
        {
            int paginaAtual = writer.PageNumber;
            var textoPaginacao = $"Página {paginaAtual} de { totalPaginas}";

            float larguraTextoPaginacao = fonteBaseRodape.GetWidthPoint(textoPaginacao, fonteRodape.Size);

            var tamanhoPagina = document.PageSize;

            wdc.BeginText();
            wdc.SetFontAndSize(fonteRodape.BaseFont, fonteRodape.Size);
            wdc.SetTextMatrix(tamanhoPagina.Width - document.RightMargin - larguraTextoPaginacao, document.BottomMargin * 0.75f);
            wdc.ShowText(textoPaginacao);
            wdc.EndText();
        }
    }
}
