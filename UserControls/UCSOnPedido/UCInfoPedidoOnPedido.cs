﻿using SysIntegradorApp.ClassesAuxiliares;
using SysIntegradorApp.ClassesAuxiliares.ClassesDeserializacaoDelmatch;
using SysIntegradorApp.ClassesAuxiliares.ClassesDeserializacaoOnPedido;
using SysIntegradorApp.ClassesDeConexaoComApps;
using SysIntegradorApp.data;
using SysIntegradorApp.Forms.ONPEDIDO;
using SysIntegradorApp.UserControls.UCSDelMatch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysIntegradorApp.UserControls.UCSOnPedido;

public partial class UCInfoPedidoOnPedido : UserControl
{
    public PedidoOnPedido Pedido { get; set; }
    public string? Status { get; set; }

    public UCInfoPedidoOnPedido()
    {
        InitializeComponent();
    }

    public void SetLabels()
    {
        try
        {
            string? TipoPedido = Pedido.Return.Type;
            string? TipoDaEntrega = "";
            string? EnderecoDaEntrega = "";
            var DataCorreta = " ";

            if (TipoPedido == "DELIVERY")
            {
                TipoDaEntrega = "Propria";
                EnderecoDaEntrega = Pedido.Return.Delivery.DeliveryAddressON.FormattedAddress;
                DataCorreta = Pedido.Return.Delivery.DeliveryDateTime;
            }

            if (TipoPedido == "TAKEOUT")
            {
                TipoDaEntrega = "Retirada";
                EnderecoDaEntrega = "RETIRADA NO LOCAL DO RESTAURANTE";

                DataCorreta = Pedido.Return.TakeOut.TakeoutDateTime;

                btnDespachar.Text = "Pronto";

            }

            if (TipoPedido == "INDOOR")
            {
                TipoDaEntrega = Pedido.Return.Indoor.Place;
                EnderecoDaEntrega = $"Entregar pedido na {Pedido.Return.Indoor.Place}";

                labelTipoEntregaNM.Text = "Entregar para:";
                tipoEntrega.Location = new Point(183, 23);

                DataCorreta = Pedido.Return.Indoor.IndoorDateTime;


            }

            if (Pedido.Return.OrderTiming == "SCHEDULED")
            {
                labelTipoEntregaNM.Text = "Agendada:";
                labelTipoEntregaNM.ForeColor = Color.Red;
                tipoEntrega.Location = new Point(155, 23);

                if (TipoPedido == "DELIVERY")
                {
                    DataCorreta = Pedido.Return.Delivery.DeliveryDateTime;
                }

                if (TipoPedido == "TAKEOUT")
                {
                    DataCorreta = Pedido.Return.TakeOut.TakeoutDateTime;
                }

                if (TipoPedido == "INDOOR")
                {
                    DataCorreta = Pedido.Return.Indoor.IndoorDateTime;
                }
            }

            labelDisplayId.Text = $"#{Pedido.Return.DisplayId.ToString()}";

            numId.Text = $"({Pedido.Return.Customer.PhoneOn.Extension}) {Pedido.Return.Customer.PhoneOn.Number}";

            label1.Text = Pedido.Return.Customer.Name;


            dateFeitoAs.Text = Pedido.Return.CreatedAt.ToString().Substring(11, 5);

            tipoEntrega.Text = TipoDaEntrega;


            horarioEntregaPrevista.Text = DataCorreta.Substring(11, 5);

            labelEndereco.Text = EnderecoDaEntrega;

            ValorTotalDosItens.Text = Pedido.Return.Total.ItemsPrice.value.ToString("c");

            float ValorEntrega = 0.0f;

            var EntregaObj = Pedido.Return.OtherFees.Where(x => x.Type == "DELIVERY_FEE").FirstOrDefault();
            ValorEntrega = EntregaObj.Price.Value;

            valorTaxaDeEntrega.Text = ValorEntrega.ToString("c");

            float ValorTaxasAdicionais = 0.0f;

            foreach (var item in Pedido.Return.OtherFees)
            {
                if (item.Type != "DELIVERY_FEE")
                {
                    ValorTaxasAdicionais += item.Price.Value;
                }
            }

            valorTaxaAdicional.Text = ValorTaxasAdicionais.ToString("c");

            float ValorDescontosNum = 0.0f;

            foreach (var item in Pedido.Return.Discounts)
            {
                ValorDescontosNum += item.Amount.value;
            }

            valorDescontos.Text = ValorDescontosNum.ToString("c");

            valorTotal.Text = Pedido.Return.Total.OrderAmount.value.ToString("c");


            var InfoPag = ClsInfosDePagamentosParaImpressaoONPedido.DefineTipoDePagamento(Pedido.Return.Payments);

            var Info1 = $"{InfoPag.FormaPagamento} ({InfoPag.TipoPagamento})";


            infoPagPedido.Text = InfoPag.TipoPagamento;
            obsPagamentoPedido.Text = InfoPag.FormaPagamento;

            if (TipoPedido == "INDOOR")
            {
                infoPagPedido.Text = "O pagamento do pedido será efetuado no caixa!";
                obsPagamentoPedido.Text = "Pedido deverá ser cobrado no caixa";
            }


            if (Pedido.Return.Customer.DocumentNumber != null || Pedido.Return.Customer.DocumentNumber.Length > 1)
            {
                if (Pedido.Return.Customer.DocumentNumber != "false")
                {
                    labelCPF.Text = Pedido.Return.Customer.DocumentNumber;
                }
                else if (Pedido.Return.Customer.DocumentNumber == "false")
                {
                    labelCPF.Text = "Não";
                }
            }
            else
            {
                labelCPF.Text = "Não";
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Ops");
        }
    }

    public void InsereItemNoPedido(List<itemsOn> items)
    {
        //primeiro instanciar um objeto do UserControl UCItem
        //Dentro do UserControl Temos que criar um método que define as labels 

        foreach (var item in items)
        {
            UCItemONPedido uCItem = new UCItemONPedido();
            uCItem.SetLabels(item.Name, item.quantity, item.TotalPrice.Value, item.OptionsPrice.Value, item.TotalPrice.Value, item.Options, uCItem, item);
            panelDeItens.Controls.Add(uCItem);
        }

    }

    private async void btnDespachar_Click(object sender, EventArgs e)
    {
        try
        {
            await OnPedido.DespachaPedido(Pedido.Return.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ops");
        }
    }

    private async void btnConcluido_Click(object sender, EventArgs e)
    {
        try
        {
            await OnPedido.ConcluirPedido(Pedido.Return.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ops");
        }
    }

    private void buttonImprimir_Click(object sender, EventArgs e)
    {
        using ApplicationDbContext db = new ApplicationDbContext();
        ParametrosDoPedido? pedido = db.parametrosdopedido.Where(x => x.Id == Pedido.Return.Id).FirstOrDefault();
        ParametrosDoSistema? opSistema = db.parametrosdosistema.ToList().FirstOrDefault();

        List<string> impressoras = new List<string>() { opSistema.Impressora1, opSistema.Impressora2, opSistema.Impressora3, opSistema.Impressora4, opSistema.Impressora5, opSistema.ImpressoraAux };

        bool ImprimeSoCaixa = pictureBoxDois.Visible == true && pictureBoxUm.Visible == false ? true : false;

        if (!opSistema.AgruparComandas && !ImprimeSoCaixa)
        {
            foreach (string imp in impressoras)
            {
                if (imp != "Sem Impressora" && imp != null)
                {
                    ImpressaoONPedido.ChamaImpressoes(pedido.Conta, pedido.DisplayId, imp);
                }
            }
        }
        else if (!ImprimeSoCaixa)
        {
            ImpressaoONPedido.ChamaImpressoesCasoSejaComandaSeparada(pedido.Conta, pedido.DisplayId, impressoras);
        }

        if (ImprimeSoCaixa)
        {
            if (opSistema.ImpCompacta)
            {
                ImpressaoONPedido.DefineImpressao2(pedido.Conta, pedido.DisplayId, opSistema.Impressora1);
            }
            else
            {
                ImpressaoONPedido.DefineImpressao(pedido.Conta, pedido.DisplayId, opSistema.Impressora1);
            }
        }


        impressoras.Clear();
    }

    private void pictureBoxUm_Click(object sender, EventArgs e)
    {
        pictureBoxUm.Visible = false;
        pictureBoxDois.Visible = true;
    }

    private void pictureBoxDois_Click(object sender, EventArgs e)
    {
        pictureBoxUm.Visible = true;
        pictureBoxDois.Visible = false;
    }

    private void button3_Click(object sender, EventArgs e)
    {
        FormCancelamentoOnPedido formDeCancelamento = new FormCancelamentoOnPedido() { IdPedido = Pedido.Return.Id};

        formDeCancelamento.ShowDialog();
    }
}
