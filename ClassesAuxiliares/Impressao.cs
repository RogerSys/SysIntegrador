﻿using SysIntegradorApp.data;
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
namespace SysIntegradorApp.ClassesAuxiliares;


public class Impressao
{
    public static int NumContas { get; set; }
    public static List<ClsImpressaoDefinicoes>? Conteudo { get; set; } = new List<ClsImpressaoDefinicoes>();


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

    public enum Alinhamentos
    {
        Esquerda,
        Direita,
        Centro
    }

    public static void Imprimir(List<ClsImpressaoDefinicoes> conteudo, string impressora1)
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

    public static void PrintPageHandler(object sender, PrintPageEventArgs e, List<ClsImpressaoDefinicoes> conteudo)
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
                    if (item.Alinhamento == Alinhamentos.Centro)
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

                    if (tamanhoFrase > e.PageBounds.Width - 65 && frase != "")
                    {
                        if (item.Alinhamento == Alinhamentos.Centro)
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
                        if (item.Alinhamento == Alinhamentos.Centro)
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


    public static void DefineImpressao(int numConta, string impressora1)
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.Conta == numConta).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonSerializer.Deserialize<PedidoCompleto>(pedidoPSQL.Json);
            ParametrosDoSistema? opcDoSistema = dbContext.parametrosdosistema.Where(x => x.Id == 1).FirstOrDefault();

            string banco = opcDoSistema.CaminhodoBanco;
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.orderType == "TAKEOUT" ? "Retirada" : "Entrega Propria";


                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {

                    AdicionaConteudo($"{opcDoSistema.NomeFantasia}", FonteNomeRestaurante, Alinhamentos.Centro);
                    AdicionaConteudo($"{opcDoSistema.Endereco}", FonteGeral);
                    AdicionaConteudo($"{opcDoSistema.Telefone}", FonteGeral, Alinhamentos.Centro);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº000\n", FonteNomeDoCliente);
                    AdicionaConteudo($"{defineEntrega}\n", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Código de coleta: {pedidoCompleto.delivery.pickupCode}", FonteItens);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


                    AdicionaConteudo("Origem: \t\t       Ifood", FonteGeral);
                    AdicionaConteudo("Atendente: \t      SysIntegrador", FonteGeral);
                    AdicionaConteudo($"Realizado: \t {pedidoCompleto.createdAt.Substring(0, 10)} {pedidoCompleto.createdAt.Substring(11, 5)}", FonteGeral);

                    if (defineEntrega == "Retirada")
                    {
                        AdicionaConteudo($"Terminar Até: \t {pedidoCompleto.takeout.takeoutDateTime.Substring(0, 10)} {pedidoCompleto.takeout.takeoutDateTime.Substring(11, 5)}", FonteGeral);
                    }
                    else
                    {
                        AdicionaConteudo($"Entregar Até: \t {pedidoCompleto.delivery.deliveryDateTime.Substring(0, 10)} {pedidoCompleto.delivery.deliveryDateTime.Substring(11, 5)}", FonteGeral);
                    }

                    AdicionaConteudo($"Realizado: \t {pedidoCompleto.createdAt.Substring(0, 10)} {pedidoCompleto.createdAt.Substring(11, 5)}", FonteGeral);
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
                        AdicionaConteudo($"{item.quantity}X {item.name} {item.totalPrice.ToString("c")}\n\n", FonteItens);
                        if (item.options != null)
                        {
                            foreach (var option in item.options)
                            {
                                AdicionaConteudo($"{option.quantity}X {option.name} {option.price.ToString("c")}", FonteDetalhesDoPedido);
                            }

                            if (item.observations != null)
                            {
                                AdicionaConteudo($"Obs: {item.observations}", FonteCPF);
                            }

                            valorDosItens += item.totalPrice;
                        }

                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"Valor dos itens: \t  {valorDosItens.ToString("c")} ", FonteTotaisDoPedido);
                    AdicionaConteudo($"Taxa De Entrega: \t  {pedidoCompleto.total.deliveryFee.ToString("c")}", FonteTotaisDoPedido);
                    AdicionaConteudo($"Taxa Adicional: \t ", FonteTotaisDoPedido);
                    AdicionaConteudo($"Descontos:      \t   {pedidoCompleto.total.benefits.ToString("c")}", FonteTotaisDoPedido);
                    AdicionaConteudo($"Valor Total: \t {pedidoCompleto.total.orderAmount.ToString("c")}", FonteTotaisDoPedido);
                    valorDosItens = 0f;
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    if (pedidoCompleto.delivery.observations != null)
                    {
                        AdicionaConteudo($"{pedidoCompleto.delivery.observations}", FonteObservaçõesItem);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo(DefineTipoDePagamento(pedidoCompleto.payments.methods), FonteGeral);
                    AdicionaConteudo($"Valor: \t {pedidoCompleto.payments.prepaid.ToString("c")}", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);


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


    public static void ImprimeComanda(int numConta, string impressora1)
    {
        try
        {
            //fazer select no banco de dados de parâmetros do pedido aonde o num contas sejá relacionado com ele
            using ApplicationDbContext dbContext = new ApplicationDbContext();
            ParametrosDoPedido? pedidoPSQL = dbContext.parametrosdopedido.Where(x => x.Conta == numConta).FirstOrDefault();
            PedidoCompleto? pedidoCompleto = JsonSerializer.Deserialize<PedidoCompleto>(pedidoPSQL.Json);

            string banco = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\gui-c\OneDrive\Área de Trabalho\SysIntegrador\CONTAS.mdb";
            string sqlQuery = $"SELECT * FROM Contas where CONTA = {numConta}";

            using (OleDbConnection connection = new OleDbConnection(banco))
            {
                connection.Open();
                string? defineEntrega = pedidoCompleto.delivery.deliveredBy == null ? "Retirada" : "Entrega Propria";


                using (OleDbCommand comando = new OleDbCommand(sqlQuery, connection))
                using (OleDbDataReader reader = comando.ExecuteReader())
                {
                    //nome do restaurante estatico por enquanto

                    AdicionaConteudo($"Pedido: \t#{pedidoCompleto.displayId}", FonteNúmeroDoPedido);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    AdicionaConteudo($"Entrega: \t  Nº000\n", FonteNomeDoCliente);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                    int qtdItens = pedidoCompleto.items.Count();
                    int contagemItemAtual = 1;
                    foreach (var item in pedidoCompleto.items)
                    {
                        AdicionaConteudo($"Item: {contagemItemAtual}/{qtdItens}", FonteItens);
                        AdicionaConteudo($"{item.quantity}X {item.name}", FonteItens);
                        if (item.options != null)
                        {
                            foreach (var option in item.options)
                            {
                                AdicionaConteudo($"{option.quantity}X {option.name}", FonteDetalhesDoPedido);
                            }

                        }
                        contagemItemAtual++;
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }
                    contagemItemAtual = 0;

                    if (pedidoCompleto.delivery.observations != null)
                    {
                        AdicionaConteudo($"{pedidoCompleto.delivery.observations}", FonteObservaçõesItem);
                        AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);
                    }

                    AdicionaConteudo($"{pedidoCompleto.delivery.observations}", FonteObservaçõesItem);


                    AdicionaConteudo("Impresso por:", FonteGeral);
                    AdicionaConteudo("SysMenu / SysIntegrador", FonteGeral);
                    AdicionaConteudo(AdicionarSeparador(), FonteSeparadores);

                }

                Imprimir(Conteudo, impressora1);
                Conteudo.Clear();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao imprimir comanda!", "Ops");
        }
    }

    public static void ChamaImpressoes(int numConta, string impressora1)
    {
        ImprimeComanda(numConta, impressora1);
        DefineImpressao(numConta, impressora1);
    }


    public static string AdicionarSeparador()
    {
        return "───────────────────────────";
    }

    public static string DefineTipoDePagamento(List<Methods> metodos)
    {
        string formaDePagamento = "";
        string tipoDePagamento = "";
        foreach (Methods metodo in metodos)
        {
            switch (metodo.type)
            {
                case "ONLINE":
                    tipoDePagamento = "Pago Online";
                    break;
            }

           
               switch (metodo.method)
            {
                case "CREDIT":
                    formaDePagamento = "(Crédito)";
                    break;
                case "MEAL_VOUCHER":
                    formaDePagamento = "(VOUCHER)";
                    break;
                case "DEBIT":
                    formaDePagamento = "(Débito)";
                    break;
                case "PIX":
                    formaDePagamento = "(PIX)";
                    break;
                case "CASH":
                    formaDePagamento = "(Dinheiro)";
                    break;
                case "BANK_PAY ":
                    formaDePagamento = "(Bank Pay)";
                    break;
                case "FOOD_VOUCHER ":
                    formaDePagamento = "(Vale Refeição)";
                    break;
                default:
                    formaDePagamento = "(Online)";
                    break;
            
        }

        }

        return tipoDePagamento += " " + formaDePagamento;
    }

    public static void AdicionaConteudo(string conteudo, Font fonte, Alinhamentos alinhamento = Alinhamentos.Esquerda)
    {
        Conteudo.Add(new ClsImpressaoDefinicoes() { Texto = conteudo, Fonte = fonte, Alinhamento = alinhamento });
    }


}