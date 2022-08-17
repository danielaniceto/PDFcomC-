using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text.Json;
using GeradorDeRelatoriosEmPdf;
using System.Diagnostics;

namespace GeradorDeRelatoriosEmPDF
{
    class Program
    {
        static List<Pessoa> pessoas = new List<Pessoa>();
        static BaseFont fonteBase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        static void Main(string[] args)
        {
            DesserializarPessoas();
            GerarRelatorioEmPDF(100);
        }

        static void DesserializarPessoas()
        {
            if (File.Exists("pessoas.json"))
            {
                using (var sr = new StreamReader("pessoas.json"))
                {
                    var dados = sr.ReadToEnd();
                    pessoas = JsonSerializer.Deserialize(dados, typeof(List<Pessoa>)) as List<Pessoa>;
                }
            }
        }

        static void GerarRelatorioEmPDF(int qtedePessoas)
        {
            var pessoasSelecionadas = pessoas.Take(qtedePessoas).ToList();
            if (pessoasSelecionadas.Count > 0)
            {
                //calculo da quantidade de paginas total
                int totalPaginas = 1;
                int totalLinhas = pessoasSelecionadas.Count;
                if (totalLinhas > 17)
                    totalPaginas += (int)Math.Ceiling((totalLinhas - 17) / 21F);

                //configuração do documento PDF
                var pxPorMm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4, 15 * pxPorMm, 15 * pxPorMm, 15 * pxPorMm, 20 * pxPorMm);
                var nomeArquivo = $"pessoas.{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")}.pdf";
                var arquivo = new FileStream(nomeArquivo, FileMode.Create);
                var writer = PdfWriter.GetInstance(pdf, arquivo);
                writer.PageEvent = new EventoDePagina(totalPaginas);
                pdf.Open();

                //adição do titulo
                var fonteParagrafo = new iTextSharp.text.Font(fonteBase, 32, iTextSharp.text.Font.NORMAL, BaseColor.Black);
                var titulo = new Paragraph("Relação de Pessoas\n\n", fonteParagrafo);
                titulo.Alignment = Element.ALIGN_LEFT;
                titulo.SpacingAfter = 4;
                pdf.Add(titulo);

                //abre o PDF no visualizador padrão
                var caminhoPDF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo);
                if (File.Exists(caminhoPDF))
                {
                    Process.Start(new ProcessStartInfo() { Arguments = $"/c start{caminhoPDF}", FileName = "cmd.exe", CreateNoWindow = true });

                }

                // adição da imagem
                var caminhoImagem = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img\\youtube.png");
                if (File.Exists(caminhoPDF))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(caminhoImagem);
                    float razaoAlturaLargura = logo.Width / logo.Height;
                    float alturaLogo = 32;
                    float larguraLogo = alturaLogo * razaoAlturaLargura;
                    logo.ScaleToFit(larguraLogo, alturaLogo);
                    var margemEsquerda = pdf.PageSize.Width - pdf.RightMargin - larguraLogo;
                    var margemTopo = pdf.PageSize.Height - pdf.TopMargin - 54;
                    logo.SetAbsolutePosition(margemEsquerda, margemTopo);
                    writer.DirectContent.AddImage(logo, false);
                }

                //adição de um link
                var fonteLink = new iTextSharp.text.Font(fonteBase, 9.9F, Font.NORMAL, BaseColor.Blue);
                var link = new Chunk("LinkedIn Daniel Gustavo Aniceto", fonteLink);
                link.SetAnchor("https://www.linkedin.com/in/daniel-gustavo-aniceto-63908710b/");
                var larguraTextLink = fonteBase.GetWidthPoint(link.Content, fonteLink.Size);
                var caixaTexto = new ColumnText(writer.DirectContent);
                caixaTexto.AddElement(link);
                caixaTexto.SetSimpleColumn(
                    pdf.PageSize.Width - pdf.RightMargin - larguraTextLink,
                    pdf.PageSize.Height - pdf.TopMargin - (18 * pxPorMm),
                    pdf.PageSize.Width - pdf.RightMargin,
                    pdf.PageSize.Width - pdf.RightMargin - (18 * pxPorMm));
                caixaTexto.Go();

                //adição da tabela de dados
                //PdfPTable tabela = new PdfPTable(5); -- pode ser feito assim tbm, por boa prática
                var tabela = new PdfPTable(5);
                float[] largurasColunas = { 0.6f, 2f, 1.5f, 1f, 1f };
                tabela.SetTotalWidth(largurasColunas);
                tabela.DefaultCell.BorderWidth = 0;
                tabela.WidthPercentage = 100;

                //adição das celulas de títulos das colunas
                CriarCelulaTexto(tabela, "Codigo", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Nome", PdfCell.ALIGN_LEFT, true);
                CriarCelulaTexto(tabela, "Profissão", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Salário", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Empregada", PdfCell.ALIGN_CENTER, true);
                                     
                foreach (var p in pessoasSelecionadas)
                {
                    CriarCelulaTexto(tabela, p.IdPessoa.ToString("D6"), PdfCell.ALIGN_LEFT, true);
                    CriarCelulaTexto(tabela, p.Nome + " " + p.Sobrenome);
                    CriarCelulaTexto(tabela, p.Profissao.Nome, PdfCell.ALIGN_LEFT, true);
                    CriarCelulaTexto(tabela, p.Salario.ToString("C2"), PdfCell.ALIGN_LEFT, true);
                    //CriarCelulaTexto(tabela, p.Empregado ? "Sim" : "Não", PdfCell.ALIGN_LEFT, true);
                    var caminhoImagemCelula = p.Empregado ? "img\\emoji_feliz.png" : "img\\emoji_triste.png ";
                    caminhoImagemCelula = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caminhoImagemCelula);
                    CriarCelulaImagem(tabela, caminhoImagemCelula, 20, 20);
                }

                pdf.Add(tabela);

                pdf.Close();
                arquivo.Close();

            }
        }
        static void CriarCelulaTexto(PdfPTable tabela, string texto, int alinhamentoHorz = PdfPCell.ALIGN_LEFT, bool negrito = false, bool italico = false, int tamanhoFonte = 12, int alturaCelula = 25)
        {
            int estilo = iTextSharp.text.Font.NORMAL;
            if(negrito && italico)
            {
                estilo = iTextSharp.text.Font.BOLDITALIC;
            }
            else if (negrito)
            {
                estilo=iTextSharp.text.Font.BOLD;
            }
            else if (italico)
            {
                estilo = iTextSharp.text.Font.ITALIC;
            }
            var fonteCelula = new iTextSharp.text.Font(fonteBase, 12, iTextSharp.text.Font.NORMAL, BaseColor.Black);

            var bgColor = iTextSharp.text.BaseColor.White;
            if (tabela.Rows.Count % 2 == 1)
                bgColor = new BaseColor(0.90F, 0.90F, 0.90F);

            var celula = new PdfPCell(new Phrase(texto, fonteCelula));
            celula.HorizontalAlignment = alinhamentoHorz;
            celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celula.Border = 0;
            celula.BorderWidthBottom = 1;
            celula.BackgroundColor = bgColor;
            tabela.AddCell(celula);
        }
        static void CriarCelulaImagem(PdfPTable tabela, string caminhoImagem, int larguraImagem, int alturaImagem, int alturaCelula = 35)
        {
            var bgColor = iTextSharp.text.BaseColor.White;
            if (tabela.Rows.Count % 2 == 1)
                bgColor = new BaseColor(0.90F, 0.90F, 0.90F);

            if (File.Exists(caminhoImagem))
            {
                iTextSharp.text.Image imagem = iTextSharp.text.Image.GetInstance(caminhoImagem);
                imagem.ScaleToFit(larguraImagem, alturaImagem);
            
                var celula = new PdfPCell(imagem);
                celula.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                celula.Border = 0;
                celula.BorderWidthBottom = 1;
                celula.FixedHeight = alturaCelula;
                celula.PaddingBottom = 5;
                celula.BackgroundColor = bgColor;
                tabela.AddCell(celula);
            }
        }
    }
}
