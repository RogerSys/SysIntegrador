﻿using SysIntegradorApp.ClassesAuxiliares.ClassesDeserializacaoDelmatch;
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
using static System.Windows.Forms.LinkLabel;
using Newtonsoft.Json;

namespace SysIntegradorApp.ClassesAuxiliares;

public class ImpressaoDelMatch
{
    public static int NumContas { get; set; }
    public static List<ClsImpressaoDefinicoesDelMatch>? Conteudo { get; set; } = new List<ClsImpressaoDefinicoesDelMatch>();
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
    public static Font FonteEndereçoDoCliente = new Font("DejaVu sans mono", 13, FontStyle.Bold);
    public static Font FonteItens = new Font("DejaVu sans mono", 12, FontStyle.Bold);
    public static Font FonteOpcionais = new Font("DejaVu sans mono", 11, FontStyle.Regular);
    public static Font FonteObservaçõesItem = new Font("DejaVu sans mono", 11, FontStyle.Bold);
    public static Font FonteTotaisDoPedido = new Font("DejaVu sans mono", 10, FontStyle.Bold);
    public static Font FonteCPF = new Font("DejaVu sans mono", 8, FontStyle.Bold);

    public enum AlinhamentosDelMatch
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

    public static void Imprimir(List<ClsImpressaoDefinicoesDelMatch> conteudo, string impressora1)
    {
        // Defina o nome da impressora específica que você deseja usar
        string printerName = impressora1;
        string texto = "";
        // Crie uma instância de PrintDocument
        PrintDocument printDocument = new PrintDocument();
        printDocument.PrinterSettings.PrinterName = printerName;

        printDocument.DefaultPageSettings.PaperSize = new PaperSize("Custom", 280, 500000);
        printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);



        // Atribua um manipulador de evento para o evento PrintPage
        printDocument.PrintPage += (sender, e) => PrintPageHandler(sender, e, conteudo);

        // Inicie o processo de impressão
        printDocument.Print();
    }

    public static void PrintPageHandler(object sender, PrintPageEventArgs e, List<ClsImpressaoDefinicoesDelMatch> conteudo)
    {
        try
        {
            // Define o conteúdo a ser impresso

            int Y = 0;


            foreach (var item in conteudo)
            {
                var tamanhoFrase = e.Graphics.MeasureString(item.Texto, item.Fonte).Width;

                if (tamanhoFrase < e.PageBounds.Width)
                {
                    if (item.Alinhamento == AlinhamentosDelMatch.Centro)
                    {
                        e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                    }
                    else
                    {
                        e.Graphics.DrawString(item.Texto, item.Fonte, Brushes.Black, 0, Y);
                        Y += 24;
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
                        if (item.Alinhamento == AlinhamentosDelMatch.Centro)
                        {

                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);
                            Y += 24;
                            frase = "";
                            continue;

                        }
                        else
                        {
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);
                            Y += 24;
                            frase = "";
                            continue;
                        }

                    }



                    if (frase != "")
                    {
                        if (item.Alinhamento == AlinhamentosDelMatch.Centro)
                        {

                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, Centro(item.Texto, item.Fonte, e), Y);

                        }
                        else
                        {
                            e.Graphics.DrawString(frase, item.Fonte, Brushes.Black, 0, Y);

                        }

                    }

                }

                frase = "";
                Y += 24;
            }
            // Desenhe o texto na área de impressão



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
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcDoSistema = dbContext.parametrosdosistema.Where(x => x.Id == 1).FirstOrDefault();

            string banco = opcDoSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.Type == "TOGO" ? "Retirada" : "Entrega Propria";

                string NumContaString = numConta.ToString();

                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {

                    AdicionaConteudo($"{opcDoSistema.NomeFantasia}", FonteNomeRestaurante, AlinhamentosDelMatch.Centro);
                    AdicionaConteudo($"{opcDoSistema.Endereco}", FonteGeral);
                    AdicionaConteudo($"{opcDoSistema.Telefone}", FonteGeral, AlinhamentosDelMatch.Centro);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Pedido:  #{pedidoCompleto.Id}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo($"{defineEntrega}\n", FonteGeral);


                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Origem: \t              Del Match", FonteGeral);
                    AdicionaConteudo("Atendente: \t       SysIntegrador", FonteGeral);


                    DateTime DataCertaCriadoEmTimeStamp = DateTime.Parse(pedidoCompleto.CreatedAt);
                    var DataCertaCriadoEm = DataCertaCriadoEmTimeStamp.ToString();

                    AdicionaConteudo($"Realizado: \t {DataCertaCriadoEm.Substring(0,10)} {DataCertaCriadoEm.Substring(11,5)}", FonteGeral);

                    if (defineEntrega == "Retirada")
                    { 

                        DateTime DataCertaTerminarEmTimeStamp = DateTime.Parse(pedidoCompleto.CreatedAt);
                        var DataCertaTerminarEm = DataCertaTerminarEmTimeStamp.AddMinutes(30);

                        AdicionaConteudo($"Terminar Até: \t {DataCertaTerminarEm.ToString().Substring(0, 10)} {DataCertaTerminarEm.ToString().Substring(11, 5)}", FonteGeral);
                    }
                    else
                    {
                        DateTime DataCertaEntregarEmTimeStamp = DateTime.Parse(pedidoCompleto.CreatedAt);
                        var DataCertaEntregarEm = DataCertaEntregarEmTimeStamp.AddMinutes(50);

                        AdicionaConteudo($"Entregar Até: \t {DataCertaEntregarEm.ToString().Substring(0, 10)} {DataCertaEntregarEm.ToString().Substring(11, 5)}", FonteGeral);
                    }

                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Fone: {pedidoCompleto.Customer.Phone}", FonteNúmeroDoTelefone);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo(pedidoCompleto.Customer.Name, FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.Type == "DELIVERY")
                    {
                        AdicionaConteudo("Endereço de entrega:", FonteCPF);
                        AdicionaConteudo($"{pedidoCompleto.deliveryAddress.StreetName}, {pedidoCompleto.deliveryAddress.StreetNumber} - {pedidoCompleto.deliveryAddress.Neighboardhood}", FonteEndereçoDoCliente);


                        if (pedidoCompleto.deliveryAddress.Complement != null || pedidoCompleto.deliveryAddress.Complement != "")
                        {
                            AdicionaConteudo($"Complemento: {pedidoCompleto.deliveryAddress.Complement}", FonteEndereçoDoCliente);
                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }
                    else
                    {
                        AdicionaConteudo("RETIRADA NO BALCÃO", FonteEndereçoDoCliente);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    float valorDosItens = 0f;

                    foreach (var item in pedidoCompleto.Items)
                    {
                       ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItemDelMatch(item);

                        AdicionaConteudo($"{item.Quantity}X {CaracteristicasPedido.NomeProduto} {item.TotalPrice.ToString("c")}\n\n", FonteItens);

                       if (item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" || item.ExternalCode == "B")
                        {
                            if (item.ExternalCode == "G")
                            {
                                AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "M")
                            {
                                AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "P")
                            {
                                AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "B")
                            {
                                AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                            }

                        }

                        if (item.SubItems.Count > 0)
                        {
                            foreach (var option in CaracteristicasPedido.Observações)
                            {
                                AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                            }

                            if (item.Observations != null && item.Observations.Length > 0)
                            {
                                AdicionaConteudo($"Obs: {item.Observations}", FonteCPF);
                            }

                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Valor dos itens: \t   {pedidoCompleto.SubTotal.ToString("c")} ", FonteTotaisDoPedido);
                    AdicionaConteudo($"Taxa De Entrega: \t   {pedidoCompleto.deliveryFee.ToString("c")}", FonteTotaisDoPedido);
                    AdicionaConteudo($"Taxa Adicional:  \t   {pedidoCompleto.AdditionalFee.ToString("c")} ", FonteTotaisDoPedido);
                    AdicionaConteudo($"Descontos:      \t   {pedidoCompleto.Discount.ToString("c")}", FonteTotaisDoPedido);
                    AdicionaConteudo($"Valor Total:   \t   {pedidoCompleto.TotalPrice.ToString("c")}", FonteTotaisDoPedido);
                    valorDosItens = 0f;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.deliveryAddress.Reference != null && pedidoCompleto.deliveryAddress.Reference.Length > 0)
                    {
                        AdicionaConteudo($"{pedidoCompleto.deliveryAddress.Reference}", FonteObservaçõesItem);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }
                    var InfoPag = ClsInfosDePagamentosParaImpressaoDelMatch.DefineTipoDePagamento(pedidoCompleto.Payments);

                    var Info1 =  $"{InfoPag.FormaPagamento} ({InfoPag.TipoPagamento})";
                    var Info2 = InfoPag.TipoPagamento == "Não é nescessario receber do cliente na entrega" ?  "Não é nescessario receber do cliente na entrega" : "Receber do Cliente na entrega";

                    AdicionaConteudo(Info1, FonteGeral);
                    AdicionaConteudo(Info2, FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("DEL MATCH", FonteNomeDoCliente, AlinhamentosDelMatch.Centro);

                }

                Imprimir(Conteudo, impressora1);
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
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string banco = opcSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";
            string NumContaString = numConta.ToString();

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.Type == "TOGO" ? "Retirada" : "Entrega Propria";

                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {
                    //nome do restaurante estatico por enquanto

                    AdicionaConteudo($"Pedido:   #{pedidoCompleto.Reference}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    int qtdItens = pedidoCompleto.Items.Count();
                    int contagemItemAtual = 1;

                    foreach (var item in pedidoCompleto.Items)
                    {
                        AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);
                        ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItemDelMatch(item, true);

                        AdicionaConteudo($"{item.Quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);

                        if (item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" || item.ExternalCode == "B")
                        {
                            if (item.ExternalCode == "G")
                            {
                                AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "M")
                            {
                                AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "P")
                            {
                                AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                            }

                            if (item.ExternalCode == "B")
                            {
                                AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                            }

                        }

                        if (item.SubItems != null)
                        {
                            foreach (var option in CaracteristicasPedido.Observações)
                            {
                                AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                            }

                            if (item.Observations != null && item.Observations.Length > 0)
                            {
                                AdicionaConteudo($"Obs: {item.Observations}", FonteCPF);
                            }

                        }
                        contagemItemAtual++;
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    }
                    contagemItemAtual = 0;

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("DEL MATCH", FonteNomeDoCliente, AlinhamentosDelMatch.Centro);
                }

                Imprimir(Conteudo, impressora1);
                Conteudo.Clear();
            }
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
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string banco = opcSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";
            string NumContaString = numConta.ToString();


            string? defineEntrega = pedidoCompleto.Type == "TOGO" ? "Retirada" : "Entrega Propria";
            int contagemItemAtual = 1;

            int qtdItens = 0;

            foreach (var item in pedidoCompleto.Items)
            {
                qtdItens += 1 * item.Quantity;
            }

            //nome do restaurante estatico por enquanto
            foreach (var item in pedidoCompleto.Items)
            {
                for (var i = 0; i < item.Quantity; i++)
                {
                    AdicionaConteudo($"Pedido:  #{pedidoCompleto.Reference}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);
                   ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItemDelMatch(item, true);

                    if (item.Quantity > 1)
                    {
                        AdicionaConteudo($"1X {item.Name}\n\n", FonteItens);
                    }
                    else
                    {
                        AdicionaConteudo($"{item.Quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                    }

                    if (item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" || item.ExternalCode == "B")
                    {
                        if (item.ExternalCode == "G")
                        {
                            AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "M")
                        {
                            AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "P")
                        {
                            AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "B")
                        {
                            AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                        }

                    }

                    if (item.SubItems != null)
                    {
                        foreach (var option in CaracteristicasPedido.Observações)
                        {
                            AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                        }

                        if (item.Observations != null && item.Observations.Length > 0)
                        {
                            AdicionaConteudo($"Obs: {item.Observations}", FonteCPF);
                        }

                    }
                    contagemItemAtual++;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("DEL MATCH", FonteNomeDoCliente, AlinhamentosDelMatch.Centro);

                    Imprimir(Conteudo, impressora1);
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
            List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch> ListaDeItems = new List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch>() { new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Cz1" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Cz2" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Cz3" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Cz4" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Sem Impressora" }, new ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch() { Impressora1 = "Bar" } };


            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();

            string? defineEntrega = pedidoCompleto.Type == "TOGO" ? "Retirada" : "Entrega Propria";

            foreach (var item in pedidoCompleto.Items)
            {
                bool ePizza = item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" ? true : false;
                string externalCode = item.ExternalCode;

                if (ePizza)
                {
                    foreach (var option in item.SubItems)
                    {
                        if (!option.ExternalCode.Contains("m"))
                        {
                            List<string> LocalDeImpressaoDasPizza = ClsDeIntegracaoSys.DefineNomeImpressoraPorProduto(option.ExternalCode);

                            List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch> GruposDoItemPizza = ListaDeItems.Where(x => x.Impressora1 == LocalDeImpressaoDasPizza[0] || x.Impressora1 == LocalDeImpressaoDasPizza[1]).ToList();

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

            List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadasDelMatch> ListaLimpa = ListaDeItems.Where(x => x.Itens.Count > 0).ToList();

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


    public static void ImprimeComandaSeparada(string impressora, int displayId, List<items> itens, int numConta)
    {
        try
        {
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();
            string NumContaString = numConta.ToString();

            //List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> itemsSeparadosPorImpressao = SeparaItensParaImpressaoSeparada();
            //string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega Propria";

            //nome do restaurante estatico por enquanto

            AdicionaConteudo($"Pedido: \t#{pedidoCompleto.Reference}", FonteNúmeroDoPedido); // aqui seria o display id Arrumar
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            int qtdItens = pedidoCompleto.Items.Count();
            int contagemItemAtual = 1;


            foreach (var item in itens)
            {

                if (impressora == "Sem Impressora" || impressora == "" || impressora == null)
                {
                    throw new Exception("Uma das impressora não foi encontrada adicione ela nas configurações ou retire a impressão separada!");
                }


                ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItemDelMatch(item, true);

                AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);

                if (item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" || item.ExternalCode == "B")
                {
                    if (item.ExternalCode == "G")
                    {
                        AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                    }

                    if (item.ExternalCode == "M")
                    {
                        AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                    }

                    if (item.ExternalCode == "P")
                    {
                        AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                    }

                    if (item.ExternalCode == "B")
                    {
                        AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                    }

                }

                AdicionaConteudo($"{item.Quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                if (item.SubItems != null)
                {
                    foreach (var option in CaracteristicasPedido.Observações)
                    {
                        AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                    }

                    if (item.Observations != null && item.Observations.Length > 0)
                    {
                        AdicionaConteudo($"Obs: {item.Observations}", FonteCPF);
                    }

                }

                AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
            }
            contagemItemAtual = 0;

            AdicionaConteudo("Impresso por:", FonteGeral);
            AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
            AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

            AdicionaConteudo("DEL MATCH", FonteNomeDoCliente, AlinhamentosDelMatch.Centro);

            if (impressora != "Nao")
            {
                Imprimir(Conteudo, impressora);
            }

            //Imprimir(Conteudo, impressora);
            Conteudo.Clear();

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    public static void ImprimeComandaSeparadaTipo2(string impressora, int displayId, List<items> itens, int numConta)
    {
        try
        {

            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.DisplayId == displayId).FirstOrDefault();
            PedidoDelMatch? pedidoCompleto = JsonConvert.DeserializeObject<PedidoDelMatch>(pedidoPSQL.Json);
            ParametrosDoSistema? opcSistema = dbContext.parametrosdosistema.ToList().FirstOrDefault();
            string NumContaString = numConta.ToString();

            //List<ClsDeSuporteParaImpressaoDosItensEmComandasSeparadas> itemsSeparadosPorImpressao = SeparaItensParaImpressaoSeparada();
            //string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega Propria";
            int contagemItemAtual = 1;

            //nome do restaurante estatico por enquanto
            foreach (var item in itens)

            {
                int quantidadeDoItem = Convert.ToInt32(item.Quantity);

                for (int i = 0; i < quantidadeDoItem; i++)
                {
                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.Reference}", FonteNúmeroDoPedido); // aqui seria o display id Arrumar
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº{NumContaString.PadLeft(3, '0')}\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    int qtdItens = pedidoCompleto.Items.Count();

                    if (impressora == "Sem Impressora" || impressora == "" || impressora == null)
                    {
                        throw new Exception("Uma das impressora não foi encontrada adicione ela nas configurações ou retire a impressão separada!");
                    }


                    ClsDeSuporteParaImpressaoDosItens CaracteristicasPedido = ClsDeIntegracaoSys.DefineCaracteristicasDoItemDelMatch(item, true);

                    AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);

                    if (item.ExternalCode == "G" || item.ExternalCode == "M" || item.ExternalCode == "P" || item.ExternalCode == "B")
                    {
                        if (item.ExternalCode == "G")
                        {
                            AdicionaConteudo(TamanhoPizza.GRANDE.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "M")
                        {
                            AdicionaConteudo(TamanhoPizza.MÉDIA.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "P")
                        {
                            AdicionaConteudo(TamanhoPizza.PEQUENA.ToString(), FonteSeparadores);
                        }

                        if (item.ExternalCode == "B")
                        {
                            AdicionaConteudo(TamanhoPizza.BROTINHO.ToString(), FonteSeparadores);
                        }

                    }

                    AdicionaConteudo($"{item.Quantity}X {CaracteristicasPedido.NomeProduto}\n\n", FonteItens);
                    if (item.SubItems != null)
                    {
                        foreach (var option in CaracteristicasPedido.Observações)
                        {
                            AdicionaConteudo($"{option}", FonteDetalhesDoPedido);
                        }

                        if (item.Observations != null && item.Observations.Length > 0)
                        {
                            AdicionaConteudo($"Obs: {item.Observations}", FonteCPF);
                        }

                    }

                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("DEL MATCH", FonteNomeDoCliente, AlinhamentosDelMatch.Centro);

                    if (impressora != "Nao")
                    {
                        Imprimir(Conteudo, impressora);
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

        DefineImpressao(numConta, displayId, opcSistema.Impressora1);
        SeparaItensParaImpressaoSeparada(numConta, displayId);
    }

    public static void ChamaImpressoes(int numConta, int displayId, string? impressora)
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ParametrosDoSistema? opcSistema = db.parametrosdosistema.ToList().FirstOrDefault();
        int ContagemDeImpressoes = 0;

        if (impressora == opcSistema.Impressora1 || impressora == opcSistema.ImpressoraAux)
        {
            DefineImpressao(numConta, displayId, impressora);
            ContagemDeImpressoes++;
            if (opcSistema.ImprimirComandaNoCaixa)
            {
                if (opcSistema.TipoComanda == 2)
                {
                    ImprimeComandaTipo2(numConta, displayId, impressora);
                }
                else
                {
                    ImprimeComanda(numConta, displayId, impressora);
                }
            }
        }
        if (ContagemDeImpressoes == 0)
        {
            if (opcSistema.TipoComanda == 2)
            {
                ImprimeComandaTipo2(numConta, displayId, impressora);
            }
            else
            {
                ImprimeComanda(numConta, displayId, impressora);
            }
        }

        ContagemDeImpressoes = 0;
    }


    public static string AdicionarSeparador()
    {
        return "───────────────────────────";
    }

    

    public static void AdicionaConteudo(string conteudo, Font fonte, AlinhamentosDelMatch alinhamento = AlinhamentosDelMatch.Esquerda)
    {
        Conteudo.Add(new ClsImpressaoDefinicoesDelMatch() { Texto = conteudo, Fonte = fonte, Alinhamento = alinhamento });
    }

    public static void AdicionaConteudoParaImpSeparada(string impressora, string conteudo, Font fonte, AlinhamentosDelMatch alinhamento = AlinhamentosDelMatch.Esquerda)
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
