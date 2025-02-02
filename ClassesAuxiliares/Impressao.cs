﻿using Newtonsoft.Json;
using SysIntegradorApp.ClassesAuxiliares.ClassesDeserializacaoDelmatch;
using SysIntegradorApp.data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SysIntegradorApp.ClassesAuxiliares.ImpressaoDelMatch;
using static System.Windows.Forms.LinkLabel;
namespace SysIntegradorApp.ClassesAuxiliares;


public class Impressao
{
    public static int NumContas { get; set; }
    public static List<ClsImpressaoDefinicoes>? Conteudo { get; set; } = new List<ClsImpressaoDefinicoes>();
    public static List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> ConteudoParaImpSeparada { get; set; } = new List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas>();


    public static Font FonteGeral = new Font("DejaVu sans mono mono", 11, FontStyle.Bold);
    public static Font FonteSeparadores = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteCódigoDeBarras = new Font("3 of 9 Barcode", 35, FontStyle.Regular);
    public static Font FonteNomeRestaurante = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public static Font FonteEndereçoDoRestaurante = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public static Font FonteNúmeroDoPedido = new Font("DejaVu sans mono", 17, FontStyle.Bold);
    public static Font FonteDetalhesDoPedido = new Font("DejaVu sans mono", 9, FontStyle.Bold);
    public static Font FonteNúmeroDoTelefone = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteNomeDoCliente = new Font("DejaVu sans mono", 15, FontStyle.Bold);
    public static Font FonteEndereçoDoCliente = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteItens = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public static Font FonteOpcionais = new Font("DejaVu sans mono", 11, FontStyle.Regular);
    public static Font FonteObservaçõesItem = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteTotaisDoPedido = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteCPF = new Font("DejaVu sans mono", 8, FontStyle.Bold);

    public enum Alinhamentos
    {
        Esquerda,
        Direita,
        Centro
    }
    public enum TamanhoPizza
    {
        PEQUENA,
        MÉDIA,
        GRANDE,
        BROTINHO
    }

    public static void Imprimir(List<ClsImpressaoDefinicoes> conteudo, string impressora1, int espacamento)
    {

        string printerName = impressora1;

        PrintDocument printDocument = new PrintDocument();
        printDocument.PrinterSettings.PrinterName = printerName;

        printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 280, 500000);
        printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

        printDocument.PrintPage += (sender, e) => PrintPageHandler(sender, e, conteudo, espacamento);

        printDocument.Print();
    }

    public static void PrintPageHandler(object sender, PrintPageEventArgs e, List<ClsImpressaoDefinicoes> conteudo, int separacao)
    {
        try
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                bool DestacaObs = db.parametrosdosistema.FirstOrDefault().DestacarObs;


                // Define o conteúdo a ser impresso

                int Y = 0;


                foreach (var item in conteudo)
                {
                    var tamanhoFrase = e.Graphics.MeasureString(item.Texto, item.Fonte).Width;

                    if (tamanhoFrase < e.PageBounds.Width)
                    {
                        if (item.Alinhamento == Alinhamentos.Centro)
                        {
                            e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                        }
                        else if (!item.eObs || !DestacaObs)
                        {
                            e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, 0, Y);
                            Y += separacao;
                            continue;
                        }
                        else if (item.eObs && DestacaObs)
                        {
                            PointF ponto = new PointF(0, Y);

                            SizeF tamanhoTexto = e.Graphics.MeasureString(item.Texto, item.Fonte);
                            RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));

                            e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                            e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, 0, Y);

                            Y += separacao;

                            continue;
                        }
                    }


                    var listPalavras = item.Texto.Split(" ").ToList();
                    string frase = "";

                    foreach (var palavra in listPalavras)
                    {

                        frase += palavra + " ";

                        tamanhoFrase = e.Graphics.MeasureString(frase, item.Fonte).Width;

                        if (tamanhoFrase > e.PageBounds.Width - 70 && frase != "")
                        {
                            if (item.Alinhamento == Alinhamentos.Centro)
                            {

                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                                Y += separacao;
                                frase = "";
                                continue;

                            }
                            else if (!item.eObs || !DestacaObs)
                            {
                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);
                                Y += separacao;
                                frase = "";
                                continue;
                            }
                            else if (item.eObs && DestacaObs)
                            {
                                PointF ponto = new PointF(0, Y);

                                SizeF tamanhoTexto = e.Graphics.MeasureString(frase, item.Fonte);
                                RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));

                                e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                                Y += separacao;
                                frase = "";

                                continue;
                            }

                        }

                        if (frase != "")
                        {
                            if (item.Alinhamento == Alinhamentos.Centro)
                            {

                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);

                            }
                            else if (!item.eObs || !DestacaObs)
                            {
                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                            }
                            else if (item.eObs && DestacaObs)
                            {
                                PointF ponto = new PointF(0, Y);

                                SizeF tamanhoTexto = e.Graphics.MeasureString(frase, item.Fonte);
                                RectangleF retanguloTexto = new RectangleF(ponto, new SizeF(e.PageBounds.Width, tamanhoTexto.Height));


                                e.Graphics.FillRectangle(Brushes.LightSlateGray, retanguloTexto);
                                e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                                //continue;
                            }

                        }

                    }

                    frase = "";
                    Y += separacao;
                }

            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        conteudo.Clear();
    }

    public static float Centro(string Texto, Font Fonte, System.Drawing.Printing.PrintPageEventArgs e)
    {
        SizeF Tamanho = e.Graphics.MeasureString(Texto, Fonte);

        float Meio = e.PageBounds.Width / 2 - Tamanho.Width / 2;

        return Meio;
    }

    public static void DefineImpressao(int numConta, int displayId, string impressora1) //impressão caixa
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcDoSistema = dbContext.parametrosdosistema.Where(x => x.Id == 1).FirstOrDefault();

            string banco = opcDoSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.orderType == "TAKEOUT" ? "Retirada" : "Entrega Propria";

                string NumContaString = numConta.ToString();

                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {
                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);

                    AdicionaConteudo($"{opcDoSistema.NomeFantasia}", FonteNomeRestaurante, Alinhamentos.Centro);
                    AdicionaConteudo($"{opcDoSistema.Endereco}", FonteGeral);
                    AdicionaConteudo($"{opcDoSistema.Telefone}", FonteGeral, Alinhamentos.Centro);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo($"{defineEntrega}\n", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Código de coleta: {pedidoCompleto.delivery.pickupCode}", FonteItens);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


                    AdicionaConteudo("Origem: \t\t       Ifood", FonteGeral);
                    AdicionaConteudo("Atendente: \t      SysIntegrador", FonteGeral);
                    DateTime DataCertaDaFeitoEmTimeStamp = DateTime.ParseExact(pedidoCompleto.createdAt, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                             System.Globalization.CultureInfo.InvariantCulture,
                                             System.Globalization.DateTimeStyles.AssumeUniversal);
                    DateTime DataCertaDaFeitoEm = DataCertaDaFeitoEmTimeStamp.ToLocalTime();

                    AdicionaConteudo($"Realizado: \t {DataCertaDaFeitoEm.ToString().Substring(0, 10)} {DataCertaDaFeitoEm.ToString().Substring(11, 5)}", FonteGeral);


                    if (defineEntrega == "Retirada")
                    {
                        DateTime DataCertaDaRetiradaemTimeStamp = DateTime.ParseExact(pedidoCompleto.takeout.takeoutDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.AssumeUniversal);
                        DateTime DataCertaDaRetirada = DataCertaDaRetiradaemTimeStamp.ToLocalTime();
                        AdicionaConteudo($"Terminar Até: \t {DataCertaDaRetirada.ToString().Substring(0, 10)} {DataCertaDaRetirada.ToString().Substring(11, 5)}", FonteGeral);
                    }
                    else
                    {
                        DateTime DataCertaDaEntregaemTimeStamp = DateTime.ParseExact(pedidoCompleto.delivery.deliveryDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.AssumeUniversal);
                        DateTime DataCertaDaEntrega = DataCertaDaEntregaemTimeStamp.ToLocalTime();
                        AdicionaConteudo($"Entregar Até: \t {DataCertaDaEntrega.ToString().Substring(0, 10)} {DataCertaDaEntrega.ToString().Substring(11, 5)}", FonteGeral);
                    }

                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Fone: {pedidoCompleto.customer.phone.number}", FonteNúmeroDoTelefone);
                    AdicionaConteudo($"Localizador: {pedidoCompleto.customer.phone.localizer}", FonteNúmeroDoTelefone);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo(pedidoCompleto.customer.name, FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.orderType == "DELIVERY")
                    {
                        AdicionaConteudo("Endereço de enntrega:", FonteCPF);
                        AdicionaConteudo($"{pedidoCompleto.delivery.deliveryAddress.formattedAddress} - {pedidoCompleto.delivery.deliveryAddress.neighborhood}", FonteEndereçoDoCliente);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }
                    else
                    {
                        AdicionaConteudo("RETIRADA NO BALCÃO", FonteEndereçoDoCliente);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    float valorDosItens = 0f;

                    foreach (var item in pedidoCompleto.items)
                    {
                        ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item);

                        AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto} {item.totalPrice.ToString("c")}\n\n", FonteItens);

                        if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                        {
                            if (item.externalCode == "G")
                            {
                                AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "M")
                            {
                                AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "P")
                            {
                                AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "B")
                            {
                                AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                            }

                        }

                        if (!opcDoSistema.RemoveComplementos)
                        {
                            if (item.options != null)
                            {
                                foreach (var option in CaracteristicasPedido.Observações)
                                {
                                    AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                                }

                                if (item.observations != null && item.observations.Length > 0)
                                {
                                    AdicionaConteudo($"Obs: {item.observations}", FonteCPF);
                                }

                                valorDosItens += item.totalPrice;
                            }
                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Valor dos itens:    \t {pedidoCompleto.total.subTotal.ToString("c")} ", FonteGeral);
                    if (pedidoCompleto.total.deliveryFee > 0)
                        AdicionaConteudo($"Taxa De Entrega:  \t {pedidoCompleto.total.deliveryFee.ToString("c")}", FonteGeral);
                    if (pedidoCompleto.total.additionalFees > 0)
                        AdicionaConteudo($"Taxa Adicional:   \t {pedidoCompleto.total.additionalFees.ToString("c")} ", FonteGeral);
                    if (pedidoCompleto.total.benefits > 0)
                        AdicionaConteudo($"Descontos:        \t\t {pedidoCompleto.total.benefits.ToString("c")}", FonteGeral);
                    AdicionaConteudo($"Valor Total:      \t\t {pedidoCompleto.total.orderAmount.ToString("c")}", FonteGeral);
                    valorDosItens = 0f;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.delivery.observations != null && pedidoCompleto.delivery.observations.Length > 0)
                    {
                        AdicionaConteudo($"{pedidoCompleto.delivery.observations}", FonteObservaçõesItem);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    ClsInfosDePagamentosParaImpressao infosDePagamento = DefineTipoDePagamento(pedidoCompleto.payments.methods);

                    AdicionaConteudo(infosDePagamento.FormaPagamento, FonteGeral);
                    AdicionaConteudo(infosDePagamento.TipoPagamento, FonteGeral);
                    if (infosDePagamento.TipoPagamento == "Pago Online")
                    {
                        AdicionaConteudo("Pedido pago online, não é nescessario receber do cliente na entrega", FonteOpcionais);
                    }

                    AdicionaConteudo($"Valor: \t {infosDePagamento.valor.ToString("c")}", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                }

                Imprimir(Conteudo, impressora1, 24);
                Conteudo.Clear();
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Ops");
        }
    }

    public static void DefineImpressao2(int numConta, int displayId, string impressora1) //impressão caixa
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcDoSistema = dbContext.parametrosdosistema.Where(x => x.Id == 1).FirstOrDefault();

            string banco = opcDoSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.orderType == "TAKEOUT" ? "R E T I R A D A" : "E N T R E G A";

                string? defineEntrega2 = pedidoCompleto.orderType == "TAKEOUT" ? "Retirada" : "Entrega";

                string NumContaString = numConta.ToString();

                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {

                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"{defineEntrega}", FonteItens);
                    AdicionaConteudo($"{opcDoSistema.NomeFantasia}", FonteItens);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Pedido:                                    #{pedidoCompleto.displayId}", FonteGeral);
                    AdicionaConteudo($"Conta Nº:                                     {NumContaString.PadLeft(3, '0')}\n", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    DateTime DataCertaDaFeitoEmTimeStamp = DateTime.ParseExact(pedidoCompleto.createdAt, "yyyy-MM-ddTHH:mm:ss.fffZ",
                          System.Globalization.CultureInfo.InvariantCulture,
                          System.Globalization.DateTimeStyles.AssumeUniversal);
                    DateTime DataCertaDaFeitoEm = DataCertaDaFeitoEmTimeStamp.ToLocalTime();


                    if (pedidoCompleto.orderTiming == "SCHEDULED")
                    {
                        AdicionaConteudo("*** PEDIDO AGENDADO ***", FonteGeral, Alinhamentos.Centro);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Realizado: \t {DataCertaDaFeitoEm.ToString().Substring(0, 10)} {DataCertaDaFeitoEm.ToString().Substring(11, 5)}", FonteGeral);

                    if (defineEntrega2 == "Retirada")
                    {
                        DateTime DataCertaDaRetiradaemTimeStamp = DateTime.ParseExact(pedidoCompleto.takeout.takeoutDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.AssumeUniversal);
                        DateTime DataCertaDaRetirada = DataCertaDaRetiradaemTimeStamp.ToLocalTime();
                        AdicionaConteudo($"Terminar Até: \t {DataCertaDaRetirada.ToString().Substring(0, 10)} {DataCertaDaRetirada.ToString().Substring(11, 5)}", FonteGeral);
                    }
                    else
                    {
                        if (pedidoCompleto.orderTiming == "SCHEDULED")
                        {
                            DateTime DataCertaDaEntregaemTimeStamp = DateTime.ParseExact(pedidoCompleto.delivery.deliveryDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.AssumeUniversal);
                            DateTime DataCertaDaEntrega = DataCertaDaEntregaemTimeStamp.ToLocalTime();
                            AdicionaConteudo($"Entregar Até: \t {DataCertaDaEntrega.ToString().Substring(0, 10)} {DataCertaDaEntrega.ToString().Substring(11, 5)}", FonteGeral);
                        }
                        else
                        {
                            DateTime DataCertaDaEntregaemTimeStamp = DateTime.ParseExact(pedidoCompleto.delivery.deliveryDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.AssumeUniversal);
                            DateTime DataCertaDaEntrega = DataCertaDaEntregaemTimeStamp.ToLocalTime();
                            AdicionaConteudo($"Entregar Até: \t {DataCertaDaEntrega.ToString().Substring(0, 10)} {DataCertaDaEntrega.ToString().Substring(11, 5)}", FonteGeral);
                        }
                    }

                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo(pedidoCompleto.customer.name, FonteItens);
                    AdicionaConteudo($"Localizador: {pedidoCompleto.customer.phone.localizer}", FonteItens);
                    AdicionaConteudo($"Fone: 0800-711-8080 ", FonteItens);

                    if (pedidoCompleto.orderType == "DELIVERY")
                    {
                        AdicionaConteudo("\n", FonteGeral);
                        AdicionaConteudo($"{pedidoCompleto.delivery.deliveryAddress.formattedAddress} - {pedidoCompleto.delivery.deliveryAddress.neighborhood}", FonteItens);


                        if (pedidoCompleto.delivery.deliveryAddress.complement != null && pedidoCompleto.delivery.deliveryAddress.complement.Length >= 1)
                        {
                            AdicionaConteudo($"Complemento: {pedidoCompleto.delivery.deliveryAddress.complement}", FonteItens);
                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }
                    else
                    {
                        AdicionaConteudo("RETIRADA NO BALCÃO", FonteItens);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    float valorDosItens = 0f;

                    foreach (var item in pedidoCompleto.items)
                    {
                        ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item);

                        AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto} {item.totalPrice.ToString("c")}\n\n", FonteItens);

                        if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                        {
                            if (item.externalCode == "G")
                            {
                                AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "M")
                            {
                                AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "P")
                            {
                                AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "B")
                            {
                                AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                            }

                        }

                        if (!opcDoSistema.RemoveComplementos)
                        {

                            if (item.options.Count > 0)
                            {
                                foreach (var option in CaracteristicasPedido.Observações)
                                {
                                    AdicionaConteudo($"{option}", FonteObservaçõesItem);
                                }
                            }
                        }

                        if (item.observations != null && item.observations.Length > 0)
                        {
                            AdicionaConteudo($"Obs: {item.observations.Replace("\n", " ")}", FonteObservaçõesItem);
                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Valor dos itens:    \t {pedidoCompleto.total.subTotal.ToString("c")} ", FonteGeral);
                    if (pedidoCompleto.total.deliveryFee > 0)
                        AdicionaConteudo($"Taxa De Entrega:  \t {pedidoCompleto.total.deliveryFee.ToString("c")}", FonteGeral);
                    if (pedidoCompleto.total.additionalFees > 0)
                        AdicionaConteudo($"Taxa Adicional:   \t {pedidoCompleto.total.additionalFees.ToString("c")} ", FonteGeral);
                    if (pedidoCompleto.total.benefits > 0)
                        AdicionaConteudo($"Descontos:        \t\t {pedidoCompleto.total.benefits.ToString("c")}", FonteGeral);
                    AdicionaConteudo($"Valor Total:      \t\t {pedidoCompleto.total.orderAmount.ToString("c")}", FonteGeral);
                    valorDosItens = 0f;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.delivery.deliveryAddress.reference != null && pedidoCompleto.delivery.deliveryAddress.reference.Length > 0)
                    {
                        AdicionaConteudo($"{pedidoCompleto.delivery.deliveryAddress.reference}", FonteGeral);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    ClsInfosDePagamentosParaImpressao infosDePagamento = DefineTipoDePagamento(pedidoCompleto.payments.methods);

                    AdicionaConteudo(infosDePagamento.FormaPagamento, FonteGeral);
                    AdicionaConteudo(infosDePagamento.TipoPagamento, FonteGeral);
                    if (infosDePagamento.TipoPagamento == "Pago Online")
                    {
                        AdicionaConteudo("Pedido pago online, não é nescessario receber do cliente na entrega", FonteGeral);
                    }

                    AdicionaConteudo($"Valor: \t {infosDePagamento.valor.ToString("c")}", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                    AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);
                    AdicionaConteudo(" ", FonteGeral, Alinhamentos.Centro);

                }

                Imprimir(Conteudo, impressora1, 16);
                Conteudo.Clear();
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Ops");
        }
    }


    public static void ImprimeComanda(int numConta, int displayId, string impressora1) //comanda
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string banco = opcSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";
            string NumContaString = numConta.ToString();

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega";

                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {
                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"{defineEntrega}: Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (opcSistema.UsarNomeNaComanda)
                    {
                        AdicionaConteudo(pedidoCompleto.customer.name, FonteItens);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    int qtdItens = pedidoCompleto.items.Count();
                    int contagemItemAtual = 1;

                    foreach (var item in pedidoCompleto.items)
                    {
                        AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);
                        ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item, true);

                        AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);

                        if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                        {
                            if (item.externalCode == "G")
                            {
                                AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "M")
                            {
                                AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "P")
                            {
                                AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                            }

                            if (item.externalCode == "B")
                            {
                                AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                            }

                        }

                        if (item.options != null)
                        {
                            foreach (var option in CaracteristicasPedido.Observações)
                            {
                                AdicionaConteudo($"{option}", FonteDetalhesDoPedido, eObs: true);
                            }

                            if (item.observations != null && item.observations.Length > 0)
                            {
                                AdicionaConteudo($"Obs: {item.observations}", FonteCPF, eObs: true);
                            }

                        }
                        contagemItemAtual++;
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    }
                    contagemItemAtual = 0;

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                    AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);
                }

                Imprimir(Conteudo, impressora1, 24);
                Conteudo.Clear();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ops");
        }
    }

    public static void ImprimeComandaReduzida(int numConta, int displayId, string impressora1) //comanda
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string NumContaString = numConta.ToString();


            string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega";

            AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            AdicionaConteudo($"{defineEntrega}: Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            if (opcSistema.UsarNomeNaComanda)
            {
                AdicionaConteudo(pedidoCompleto.customer.name, FonteItens);
                AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
            }

            foreach (var item in pedidoCompleto.items)
            {
                ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item, true);

                AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);

                if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                {
                    if (item.externalCode == "G")
                    {
                        AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "M")
                    {
                        AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "P")
                    {
                        AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "B")
                    {
                        AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                    }

                }

                if (item.options != null)
                {
                    foreach (var option in CaracteristicasPedido.Observações)
                    {
                        AdicionaConteudo($"{option}", FonteDetalhesDoPedido, eObs: true);
                    }

                    if (item.observations != null && item.observations.Length > 0)
                    {
                        AdicionaConteudo($"Obs: {item.observations}", FonteCPF, eObs: true);
                    }

                }
                AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            }

            AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
            AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);


            Imprimir(Conteudo, impressora1, 17);
            Conteudo.Clear();

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ops");
        }
    }

    public static void ImprimeComandaTipo2(int numConta, int displayId, string impressora1) //comanda
    {

        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string banco = opcSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";
            string NumContaString = numConta.ToString();


            string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega";
            int contagemItemAtual = 1;

            int qtdItens = 0;

            foreach (var item in pedidoCompleto.items)
            {
                qtdItens += 1 * item.quantity;
            }

            //nome do restaurante estatico por enquanto
            foreach (var item in pedidoCompleto.items)
            {
                for (var i = 0; i < item.quantity; i++)
                {
                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"{defineEntrega}: Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (opcSistema.UsarNomeNaComanda)
                    {
                        AdicionaConteudo(pedidoCompleto.customer.name, FonteItens);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);
                    ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item, true);

                    if (item.quantity > 1)
                    {
                        AdicionaConteudo($"1X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                    }
                    else
                    {
                        AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                    }

                    if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                    {
                        if (item.externalCode == "G")
                        {
                            AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "M")
                        {
                            AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "P")
                        {
                            AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "B")
                        {
                            AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                        }

                    }

                    if (item.options != null)
                    {
                        foreach (var option in CaracteristicasPedido.Observações)
                        {
                            AdicionaConteudo($"{option}", FonteDetalhesDoPedido, eObs: true);
                        }

                        if (item.observations != null && item.observations.Length > 0)
                        {
                            AdicionaConteudo($"Obs: {item.observations}", FonteCPF, eObs: true);
                        }

                    }
                    contagemItemAtual++;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                    AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);

                    Imprimir(Conteudo, impressora1, 24);
                    Conteudo.Clear();


                }
            }

            contagemItemAtual = 1;
            qtdItens = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ops");
        }
    }

    public static void SeparaItensParaImpressaoSeparada(int numConta, int displayId)
    {
        try
        {
            List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> ListaDeItems = new List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas>() { new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Cz1" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Cz2" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Cz3" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Cz4" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Sem Impressora" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora1 = "Bar" } };


            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();
            string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega Propria";

            foreach (var item in pedidoCompleto.items)
            {
                bool ePizza = item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B" ? true : false;
                string externalCode = item.externalCode;

                if (ePizza)
                {
                    foreach (var option in item.options)
                    {
                        if (!option.externalCode.Contains("m"))
                        {
                            List<string> LocalDeImpressaoDasPizza = ClsDeIntegracaoSys.DefineNomeImpressoraPorProduto(option.externalCode);

                            if (LocalDeImpressaoDasPizza.Count() > 1)
                            {
                                var GruposDoItemPizza = ListaDeItems.Where(x => x.Impressora1 == LocalDeImpressaoDasPizza[0] || x.Impressora1 == LocalDeImpressaoDasPizza[1]).ToList();

                                if (LocalDeImpressaoDasPizza.Count() > 1)
                                {
                                    foreach (var grupo in GruposDoItemPizza)
                                    {
                                        var verifSejaExisteAPizzaDEntroDosItens = grupo.Itens.Any(x => x == item);

                                        if (!verifSejaExisteAPizzaDEntroDosItens)
                                        {
                                            grupo.Itens.Add(item);
                                        }
                                    }
                                }
                                else
                                {
                                    var GrupoDoItem = ListaDeItems.Where(x => x.Impressora1 == LocalDeImpressaoDasPizza[0] || x.Impressora1 == LocalDeImpressaoDasPizza[1]).FirstOrDefault();

                                    if (GrupoDoItem != null)
                                    {
                                        var verifSejaExisteAPizzaDEntroDosItens = GrupoDoItem.Itens.Any(x => x == item);

                                        if (!verifSejaExisteAPizzaDEntroDosItens)
                                        {
                                            GrupoDoItem.Itens.Add(item);
                                        }

                                    }
                                }
                            }
                        }
                    }

                    continue;
                }

                //-------------------------------------------------------------------------------------------------------------------------------//
                List<string> LocalDeImpressao = ClsDeIntegracaoSys.DefineNomeImpressoraPorProduto(externalCode);

                var GruposDoItem = ListaDeItems.Where(x => x.Impressora1 == LocalDeImpressao[0] || x.Impressora1 == LocalDeImpressao[1]).ToList();

                if (GruposDoItem.Count() > 1)
                {
                    foreach (var grupo in GruposDoItem)
                    {
                        grupo.Itens.Add(item);
                    }
                }
                else
                {
                    var GrupoDoItem = ListaDeItems.Where(x => x.Impressora1 == LocalDeImpressao[0] || x.Impressora1 == LocalDeImpressao[1]).FirstOrDefault();

                    if (GrupoDoItem != null)
                    {
                        GrupoDoItem.Itens.Add(item);
                    }
                }


            }

            List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> ListaLimpa = ListaDeItems.Where(x => x.Itens.Count > 0).ToList();

            foreach (var item in ListaLimpa)
            {
                item.Impressora1 = DefineNomeDeImpressoraCasoEstejaSelecionadoImpSeparada(item.Impressora1);
                if (opcSistema.TipoComanda == 2)
                {
                    ImprimeComandaSeparadaTipo2(item.Impressora1, displayId, item.Itens, numConta);
                }
                else
                {
                    ImprimeComandaSeparada(item.Impressora1, displayId, item.Itens, numConta);
                }
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }


    public static void ImprimeComandaSeparada(string impressora, int displayId, List<Items> itens, int numConta)
    {
        try
        {
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();
            string NumContaString = numConta.ToString();

            AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);

            AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            if (opcSistema.UsarNomeNaComanda)
            {
                AdicionaConteudo(pedidoCompleto.customer.name, FonteItens);
                AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
            }

            int qtdItens = pedidoCompleto.items.Count();
            int contagemItemAtual = 1;

            foreach (var item in itens)
            {

                if (impressora == "Sem Impressora" || impressora == "" || impressora == null)
                {
                    throw new Exception("Uma das impressora não foi encontrada adicione ela nas configurações ou retire a impressão separada!");
                }


                ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item, true);

                AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);

                if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                {
                    if (item.externalCode == "G")
                    {
                        AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "M")
                    {
                        AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "P")
                    {
                        AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                    }

                    if (item.externalCode == "B")
                    {
                        AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                    }

                }

                AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                if (item.options != null)
                {
                    foreach (var option in CaracteristicasPedido.Observações)
                    {
                        AdicionaConteudo($"{option}", FonteDetalhesDoPedido, eObs: true);
                    }

                    if (item.observations != null && item.observations.Length > 0)
                    {
                        AdicionaConteudo($"Obs: {item.observations}", FonteCPF, eObs: true);
                    }

                }

                AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
            }
            contagemItemAtual = 0;

            AdicionaConteudo("Impresso por:", FonteGeral);
            AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
            AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);

            if (impressora != "Nao")
            {
                Imprimir(Conteudo, impressora, 24);
            }

            //Imprimir(Conteudo, impressora);
            Conteudo.Clear();

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro ao imprimir comanda", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void ImprimeComandaSeparadaTipo2(string impressora, int displayId, List<Items> itens, int numConta)
    {
        try
        {

            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonConvert.DeserializeObject<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();
            string NumContaString = numConta.ToString();

            //List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> itemsSeparadosPorImpressao = SeparaItensParaImpressaoSeparada();
            //string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega Propria";
            int contagemItemAtual = 1;

            //nome do restaurante estatico por enquanto
            foreach (var item in itens)

            {
                int quantidadeDoItem = Convert.ToInt32(item.quantity);

                for (int i = 0; i < quantidadeDoItem; i++)
                {
                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido); // aqui seria o display id Arrumar
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    int qtdItens = pedidoCompleto.items.Count();

                    if (impressora == "Sem Impressora" || impressora == "" || impressora == null)
                    {
                        throw new Exception("Uma das impressora não foi encontrada adicione ela nas configurações ou retire a impressão separada!");
                    }


                    ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItem(item, true);

                    AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);

                    if (item.externalCode == "G" || item.externalCode == "M" || item.externalCode == "P" || item.externalCode == "B")
                    {
                        if (item.externalCode == "G")
                        {
                            AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "M")
                        {
                            AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "P")
                        {
                            AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                        }

                        if (item.externalCode == "B")
                        {
                            AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                        }

                    }

                    AdicionaConteudo($"{item.quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                    if (item.options != null)
                    {
                        foreach (var option in CaracteristicasPedido.Observações)
                        {
                            AdicionaConteudo($"{option}", FonteDetalhesDoPedido, eObs: true);
                        }

                        if (item.observations != null && item.observations.Length > 0)
                        {
                            AdicionaConteudo($"Obs: {item.observations}", FonteCPF, eObs: true);
                        }

                    }

                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


                    AdicionaConteudo("IFOOD", FonteNomeDoCliente, Alinhamentos.Centro);
                    AdicionaConteudo("www.syslogica.com.br", FonteGeral, Alinhamentos.Centro);

                    if (impressora != "Nao")
                    {
                        Imprimir(Conteudo, impressora, 24);
                    }
                    contagemItemAtual++;
                }

            }
            //Imprimir(Conteudo, impressora);
            Conteudo.Clear();
            contagemItemAtual = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }


    public static void ChamaImpressoesCasoSejaComandaSeparada(int numConta, int displayId, List<string> impressoras)
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ParametrosDoSistema? opcSistema = db.parametrosdosistema.ToList().FirstOrDefault();

        if (opcSistema.ImpCompacta)
        {
            DefineImpressao2(numConta, displayId, opcSistema.Impressora1);
        }
        else
        {
            DefineImpressao(numConta, displayId, opcSistema.Impressora1);
        }
        SeparaItensParaImpressaoSeparada(numConta, displayId);
    }

    public static void ChamaImpressoes(int numConta, int displayId, string? impressora)
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ParametrosDoSistema? opcSistema = db.parametrosdosistema.ToList().FirstOrDefault();
        int ContagemDeImpressoes = 0;

        if (impressora == opcSistema.Impressora1 || impressora == opcSistema.ImpressoraAux)
        {
            if (opcSistema.ImpCompacta)
            {
                DefineImpressao2(numConta, displayId, impressora);
            }
            else
            {
                DefineImpressao(numConta, displayId, impressora);
            }
            ContagemDeImpressoes++;
            if (opcSistema.ImprimirComandaNoCaixa)
            {
                if (opcSistema.TipoComanda == 2)
                {
                    for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                    {
                        ImprimeComandaTipo2(numConta, displayId, impressora);
                    }
                }
                else
                {
                    if (opcSistema.ComandaReduzida)
                    {
                        for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                        {
                            ImprimeComandaReduzida(numConta, displayId, impressora);
                        }

                    }
                    else
                    {
                        for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                        {
                            ImprimeComanda(numConta, displayId, impressora);
                        }

                    }
                }
            }
        }
        if (ContagemDeImpressoes == 0)
        {
            if (opcSistema.TipoComanda == 2)
            {
                for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                {
                    ImprimeComandaTipo2(numConta, displayId, impressora);
                }
            }
            else
            {
                if (opcSistema.ComandaReduzida)
                {
                    for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                    {
                        ImprimeComandaReduzida(numConta, displayId, impressora);
                    }

                }
                else
                {
                    for (int i = 0; i < opcSistema.NumDeViasDeComanda; i++)
                    {
                        ImprimeComanda(numConta, displayId, impressora);
                    }

                }
            }
        }

        ContagemDeImpressoes = 0;
    }


    public static string AdicionarSeparador()
    {
        return "───────────────────────────";
    }

    public static ClsInfosDePagamentosParaImpressao DefineTipoDePagamento(List<Methods> metodos)
    {
        ClsInfosDePagamentosParaImpressao infos = new ClsInfosDePagamentosParaImpressao();
        foreach (Methods metodo in metodos)
        {
            switch (metodo.type)
            {
                case "ONLINE":
                    infos.TipoPagamento = "Pago Online";
                    break;
                case "OFFLINE":
                    infos.TipoPagamento = "VAI SER PAGO NA ENTREGA";
                    break;
            }


            switch (metodo.method)
            {
                case "CREDIT":
                    infos.FormaPagamento = "(Crédito)";
                    break;
                case "MEAL_VOUCHER":
                    infos.FormaPagamento = "(VOUCHER)";
                    break;
                case "DEBIT":
                    infos.FormaPagamento = "(Débito)";
                    break;
                case "PIX":
                    infos.FormaPagamento = "(PIX)";
                    break;
                case "CASH":
                    if (metodo.cash.changeFor > 0)
                    {
                        double totalTroco = metodo.cash.changeFor - metodo.value;
                        infos.FormaPagamento = $"(Dinheiro) Levar troco para {metodo.cash.changeFor.ToString("c")} Total Troco: {totalTroco.ToString("c")}";
                    }
                    else
                    {
                        infos.FormaPagamento = "(Dinheiro) Não precisa de troco";
                    }
                    break;
                case "BANK_PAY ":
                    infos.FormaPagamento = "(Bank Pay)";
                    break;
                case "FOOD_VOUCHER ":
                    infos.FormaPagamento = "(Vale Refeição)";
                    break;
                default:
                    infos.FormaPagamento = "(Online)";
                    break;

            }

            infos.valor = metodo.value;

        }

        return infos;
    }

    public static void AdicionaConteudo(string conteudo, Font fonte, Alinhamentos alinhamento = Alinhamentos.Esquerda, bool eObs = false)
    {
        Conteudo.Add(new ClsImpressaoDefinicoes() { Texto = conteudo, Fonte = fonte, Alinhamento = alinhamento, eObs = eObs });
    }

    public static void AdicionaConteudoParaImpSeparada(string impressora, string conteudo, Font fonte, Alinhamentos alinhamento = Alinhamentos.Esquerda)
    {
        //ConteudoParaImpSeparada.Add(new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas() { Impressora = impressora, conteudo = new ClsImpressaoDefinicoes() { Texto = conteudo, Fonte = fonte, Alinhamento = alinhamento } });
    }

    public static string DefineNomeDeImpressoraCasoEstejaSelecionadoImpSeparada(string LocalImpressao)
    {
        string NomeImpressora = "";
        try
        {
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            switch (LocalImpressao)
            {
                case "Cz1":
                    NomeImpressora = opcSistema.Impressora2;
                    break;
                case "Cz2":
                    NomeImpressora = opcSistema.Impressora3;
                    break;
                case "Cz3":
                    NomeImpressora = opcSistema.Impressora4;
                    break;
                case "Bar":
                    NomeImpressora = opcSistema.Impressora5;
                    break;
                case "Nao":
                    NomeImpressora = "Nao";
                    break;
                default:
                    NomeImpressora = "Sem Impressora";
                    break;
            }

        }
        catch (Exception ex)
        {

            MessageBox.Show("Erro ao Definir nome da impresora para impressão");
        }


        return NomeImpressora;
    }

}