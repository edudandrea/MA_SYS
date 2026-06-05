import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import {
  MensalidadeSistema,
  MensalidadesSistemaService,
} from '../Services/MensalidadesSistema/MensalidadesSistema.service';
import {
  PagamentoAcademia,
  PagamentoAcademiaPixResponse,
  PagamentoAcademiaStatusResponse,
  PagamentosAcademiasService,
} from '../Services/PagamentosAcademias/PagamentosAcademias.service';
import { loadMercadoPago } from '@mercadopago/sdk-js';
import * as QRCode from 'qrcode';
import { environment } from '../app/environments/environment';

@Component({
  selector: 'app-MensalidadesSistema',
  templateUrl: './MensalidadesSistema.component.html',
  styleUrls: ['./MensalidadesSistema.component.css'],
  imports: [CommonModule, FormsModule],
})
export class MensalidadesSistemaComponent implements OnInit, OnDestroy {
  modalRef?: BsModalRef;
  mensalidades: (MensalidadeSistema & { menuAberto?: boolean })[] = [];
  cobrancasAcademia: PagamentoAcademia[] = [];
  cobrancaSelecionada: PagamentoAcademia | null = null;
  metodoPagamentoSelecionado: 'pix' | 'cartao' | '' = '';
  qrCodePix = '';
  pixPayload = '';
  pagamentoMensagem = '';
  pagamentoStatus = '';
  pagamentoVerificacaoAutomatica = false;
  pixGerado = false;
  cartao = {
    numero: '',
    nome: '',
    validade: '',
    cvv: '',
    cpf: '',
  };
  editarId: number | null = null;
  role = '';
  valor = 0;
  prazoPagamentoDias = 0;
  mesesUso = 0;
  descricao = '';
  ativo = true;
  aceitaPix = true;
  aceitaCartao = true;
  mercadoPagoPublicKey = '';
  mercadoPagoAccessToken = '';
  mensalidadeSelecionada: MensalidadeSistema | null = null;
  private mp: any = null;
  private mpPublicKey = '';
  private statusPollingHandle: any = null;

  constructor(
    private mensalidadesService: MensalidadesSistemaService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private modalService: BsModalService,
    private router: Router,
    private pagamentosAcademiasService: PagamentosAcademiasService,
    private ngZone: NgZone,
  ) {}

  ngOnInit(): void {
    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    this.role = usuario.role || '';

    if (!this.canManageMensalidades() && !this.isAcademia) {
      this.toastr.warning('Tela disponivel apenas para administradores e academias.');
      this.router.navigate(['/dashboard']);
      return;
    }

    if (this.canManageMensalidades()) {
      this.carregarMensalidades();
    }

    this.carregarCobrancasAcademia();
  }

  ngOnDestroy(): void {
    this.pararPollingStatus();
  }

  get tituloPagina(): string {
    return this.isAcademia ? 'Cobranças do Sistema' : 'Mensalidades do Sistema';
  }

  get isAcademia(): boolean {
    return this.role === 'Academia';
  }

  get canManageSistema(): boolean {
    return this.canManageMensalidades();
  }

  get cobrancasRecentesAdmin(): PagamentoAcademia[] {
    return this.cobrancasAcademia.slice(0, 8);
  }

  get cobrancasPendentesAcademia(): PagamentoAcademia[] {
    return this.cobrancasAcademia.filter((item) => item.status !== 'Pago');
  }

  get cobrancaPermitePix(): boolean {
    return this.cobrancaSelecionada?.aceitaPix !== false;
  }

  get cobrancaPermiteCartao(): boolean {
    return this.cobrancaSelecionada?.aceitaCartao !== false;
  }

  openModalNovaMensalidade(template: TemplateRef<any>) {
    this.resetForm();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  toggleStatus(mensalidade: MensalidadeSistema) {
    const novoStatus = !mensalidade.ativo;

    this.mensalidadesService.atualizarStatus(mensalidade.id, novoStatus).subscribe({
      next: () => {
        mensalidade.ativo = novoStatus;
        this.mensalidades = [...this.mensalidades];
        this.cd.markForCheck();
        this.toastr.success(`Mensalidade ${novoStatus ? 'ativada' : 'desativada'}.`);
      },
      error: () => {
        this.toastr.error('Erro ao atualizar status da mensalidade.');
      },
    });
  }

  carregarMensalidades() {
    this.spinner.show();

    this.mensalidadesService.listar().subscribe({
      next: (res) => {
        this.spinner.hide();
        this.mensalidades = (res || []).map((item) => ({
          ...item,
          menuAberto: false,
          ativo: !!item.ativo,
          aceitaPix: !!item.aceitaPix,
          aceitaCartao: !!item.aceitaCartao,
        }));
        this.cd.detectChanges();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao carregar mensalidades do sistema.');
      },
    });
  }

  carregarCobrancasAcademia() {
    this.pagamentosAcademiasService.listar().subscribe({
      next: (res) => {
        this.cobrancasAcademia = res ?? [];
        this.cd.markForCheck();
      },
      error: () => {
        this.cobrancasAcademia = [];
        if (this.isAcademia) {
          this.toastr.error('Nao foi possivel carregar as cobrancas da academia.');
        }
        this.cd.markForCheck();
      },
    });
  }

  openPagamentoModal(template: TemplateRef<any>, cobranca: PagamentoAcademia) {
    this.cobrancaSelecionada = cobranca;
    this.metodoPagamentoSelecionado = '';
    this.resetPagamentoState();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });
  }

  fecharModalPagamento() {
    this.pararPollingStatus();
    this.modalRef?.hide();
  }

  selecionarMetodoPagamento(metodo: 'pix' | 'cartao') {
    if (metodo === 'pix' && !this.cobrancaSelecionada?.aceitaPix) {
      this.toastr.warning('PIX nao esta habilitado para esta cobranca.');
      return;
    }

    if (metodo === 'cartao' && !this.cobrancaSelecionada?.aceitaCartao) {
      this.toastr.warning('Cartao de credito nao esta habilitado para esta cobranca.');
      return;
    }

    this.metodoPagamentoSelecionado = metodo;
    this.resetPagamentoState();
  }

  pagarCobrancaPix() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    this.spinner.show();
    this.pagamentosAcademiasService.gerarPix(this.cobrancaSelecionada.id).subscribe({
      next: (res: PagamentoAcademiaPixResponse) => {
        this.spinner.hide();
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = res.mensagem;
        this.pixPayload = (res.payload || '').trim();
        this.pixGerado = true;
        this.qrCodePix = this.normalizarQrCodeBase64(res.qrCodeBase64);

        if (!this.qrCodePix && this.pixPayload) {
          this.gerarQrCodePix(this.pixPayload);
        }

        this.pagamentoVerificacaoAutomatica = res.verificacaoAutomaticaDisponivel;
        this.cd.detectChanges();

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento PIX confirmado com sucesso.');
          this.finalizarPagamentoConfirmado('PIX');
          return;
        }

        if (!this.qrCodePix && !this.pixPayload) {
          this.toastr.warning('PIX gerado, mas o gateway nao retornou QR Code nem codigo copia e cola.');
        } else {
          this.toastr.info('PIX gerado. Aguarde a confirmacao do pagamento.');
        }

        this.iniciarPollingStatus();
      },
      error: async (err) => {
        this.spinner.hide();
        const message = await this.extrairMensagemErroHttp(err, 'Nao foi possivel gerar o PIX da cobranca.');
        this.toastr.error(message);
      },
    });
  }

  async pagarCobrancaCartao() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    await this.inicializarMercadoPago(this.cobrancaSelecionada.mercadoPagoPublicKey);

    if (!this.mp) {
      this.toastr.error('Pagamento por cartao indisponivel. Configure a chave publica do Mercado Pago.');
      return;
    }

    if (!this.cartao.numero || !this.cartao.nome || !this.cartao.validade || !this.cartao.cvv || !this.cartao.cpf) {
      this.toastr.warning('Preencha todos os dados do cartao.');
      return;
    }

    const cardToken = await this.gerarTokenCartao();
    if (!cardToken) {
      return;
    }

    const usuario = JSON.parse(localStorage.getItem('usuario') || '{}');
    const email = usuario?.email || 'financeiro@academia.local';
    const paymentMethodId = this.detectarBandeiraMercadoPago(this.cartao.numero);

    this.spinner.show();
    this.pagamentosAcademiasService.pagarComCartao(this.cobrancaSelecionada.id, {
      payerEmail: email,
      cardToken,
      paymentMethodId,
      parcelas: 1,
    }).subscribe({
      next: (res: PagamentoAcademiaStatusResponse) => {
        this.spinner.hide();
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = this.getMensagemStatus(res.status, 'cartao');

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento com cartao aprovado.');
          this.finalizarPagamentoConfirmado('Cartao');
          return;
        }

        if (res.status === 'Recusado') {
          this.toastr.error('Pagamento com cartao recusado.');
          return;
        }

        this.toastr.info(this.pagamentoMensagem);
        this.iniciarPollingStatus();
      },
      error: async (err) => {
        this.spinner.hide();
        const message = await this.extrairMensagemErroHttp(err, 'Nao foi possivel processar o pagamento no cartao.');
        this.toastr.error(message);
      },
    });
  }

  verificarStatusCobranca() {
    if (!this.cobrancaSelecionada) {
      return;
    }

    this.pagamentosAcademiasService.consultarStatusAtualizado(this.cobrancaSelecionada.id).subscribe({
      next: (res) => {
        this.pagamentoStatus = res.status;
        this.pagamentoMensagem = this.getMensagemStatus(res.status, this.metodoPagamentoSelecionado);

        if (res.status === 'Pago') {
          this.toastr.success('Pagamento confirmado com sucesso.');
          this.finalizarPagamentoConfirmado(res.formaPagamentoNome || 'Pagamento');
          return;
        }

        if (res.status === 'Recusado') {
          this.toastr.error('Pagamento recusado.');
          this.pararPollingStatus();
        }
      },
      error: () => {
        this.toastr.error('Nao foi possivel consultar o status do pagamento.');
      },
    });
  }

  copiarPix() {
    if (!this.pixPayload) {
      this.toastr.warning('Codigo PIX ainda nao disponivel para copiar.');
      return;
    }

    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Codigo PIX copiado para a area de transferencia.');
  }

  criarMensalidade() {
    if (!this.formValido()) {
      return;
    }

    this.spinner.show();

    this.mensalidadesService.criar({
      valor: this.valor,
      prazoPagamentoDias: this.prazoPagamentoDias,
      mesesUso: this.mesesUso,
      descricao: this.descricao,
      aceitaPix: this.aceitaPix,
      aceitaCartao: this.aceitaCartao,
      mercadoPagoPublicKey: this.mercadoPagoPublicKey,
      mercadoPagoAccessToken: this.mercadoPagoAccessToken,
    }).subscribe({
      next: () => {
        this.spinner.hide();
        this.modalRef?.hide();
        this.toastr.success('Mensalidade cadastrada com sucesso.');
        this.resetForm();
        this.carregarMensalidades();
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao cadastrar mensalidade.');
      },
    });
  }

  editarMensalidade(template: TemplateRef<any>, mensalidade: MensalidadeSistema) {
    this.mensalidadeSelecionada = mensalidade;
    this.editarId = mensalidade.id;
    this.valor = mensalidade.valor;
    this.prazoPagamentoDias = mensalidade.prazoPagamentoDias;
    this.mesesUso = mensalidade.mesesUso;
    this.descricao = mensalidade.descricao || '';
    this.ativo = mensalidade.ativo;
    this.aceitaPix = mensalidade.aceitaPix;
    this.aceitaCartao = mensalidade.aceitaCartao;
    this.mercadoPagoPublicKey = mensalidade.mercadoPagoPublicKey || '';
    this.mercadoPagoAccessToken = '';
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  salvarEdicao(mensalidade: MensalidadeSistema) {
    if (!this.formValido()) {
      return;
    }

    this.mensalidadesService.atualizar({
      id: mensalidade.id,
      valor: this.valor,
      prazoPagamentoDias: this.prazoPagamentoDias,
      mesesUso: this.mesesUso,
      descricao: this.descricao,
      ativo: this.ativo,
      aceitaPix: this.aceitaPix,
      aceitaCartao: this.aceitaCartao,
      mercadoPagoPublicKey: this.mercadoPagoPublicKey,
      mercadoPagoAccessToken: this.mercadoPagoAccessToken,
    }).subscribe({
      next: () => {
        mensalidade.valor = this.valor;
        mensalidade.prazoPagamentoDias = this.prazoPagamentoDias;
        mensalidade.mesesUso = this.mesesUso;
        mensalidade.descricao = this.descricao;
        mensalidade.ativo = this.ativo;
        mensalidade.aceitaPix = this.aceitaPix;
        mensalidade.aceitaCartao = this.aceitaCartao;
        mensalidade.mercadoPagoPublicKey = this.mercadoPagoPublicKey;
        this.editarId = null;
        this.mensalidadeSelecionada = null;
        this.modalRef?.hide();
        this.resetForm();
        this.toastr.success('Mensalidade atualizada com sucesso.');
        this.carregarMensalidades();
      },
      error: () => {
        this.toastr.error('Erro ao atualizar mensalidade.');
      },
    });
  }

  cancelarEdicao() {
    this.editarId = null;
    this.mensalidadeSelecionada = null;
    this.modalRef?.hide();
    this.resetForm();
  }

  salvarModal() {
    if (this.editarId && this.mensalidadeSelecionada) {
      this.salvarEdicao(this.mensalidadeSelecionada);
      return;
    }

    this.criarMensalidade();
  }

  formatarValor(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(valor || 0);
  }

  private canManageMensalidades(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  private async inicializarMercadoPago(publicKeyOverride?: string | null) {
    const publicKey = publicKeyOverride || environment.mercadoPagoPublicKey;
    if (!publicKey || publicKey.includes('__CONFIGURE_VIA_')) {
      this.mp = null;
      this.mpPublicKey = '';
      return;
    }

    if (this.mp && this.mpPublicKey === publicKey) {
      return;
    }

    await loadMercadoPago();
    this.mp = new (window as any).MercadoPago(publicKey, { locale: 'pt-BR' });
    this.mpPublicKey = publicKey;
  }

  private gerarQrCodePix(payload: string) {
    QRCode.toDataURL(payload)
      .then((url) => {
        this.ngZone.run(() => {
          this.qrCodePix = url;
          this.cd.detectChanges();
        });
      })
      .catch(() => {
        this.ngZone.run(() => {
          this.qrCodePix = '';
          this.toastr.error('Nao foi possivel gerar o QR Code do PIX.');
          this.cd.detectChanges();
        });
      });
  }

  private normalizarQrCodeBase64(qrCodeBase64?: string | null): string {
    const valor = (qrCodeBase64 || '').trim();
    if (!valor) {
      return '';
    }

    if (valor.startsWith('data:image/')) {
      return valor;
    }

    return `data:image/png;base64,${valor}`;
  }

  private async extrairMensagemErroHttp(error: any, fallback: string): Promise<string> {
    const body = error?.error;

    if (typeof body === 'string') {
      return body || fallback;
    }

    if (body?.message) {
      return body.message;
    }

    if (body instanceof Blob) {
      const text = await body.text();
      if (!text) {
        return fallback;
      }

      try {
        const parsed = JSON.parse(text);
        return parsed?.message || text || fallback;
      } catch {
        return text;
      }
    }

    return error?.message || fallback;
  }

  private async gerarTokenCartao(): Promise<string | null> {
    try {
      const validade = (this.cartao.validade || '').split('/');
      if (validade.length !== 2) {
        this.toastr.warning('Validade do cartao deve estar no formato MM/AA.');
        return null;
      }

      const cpfNumerico = String(this.cartao.cpf || '').replace(/\D/g, '');
      if (cpfNumerico.length !== 11) {
        this.toastr.warning('Informe um CPF valido para tokenizar o cartao.');
        return null;
      }

      const token = await this.mp.createCardToken({
        cardNumber: this.cartao.numero.replace(/\s/g, ''),
        cardholderName: this.cartao.nome,
        cardExpirationMonth: validade[0],
        cardExpirationYear: `20${validade[1]}`,
        securityCode: this.cartao.cvv,
        identificationType: 'CPF',
        identificationNumber: cpfNumerico,
      });

      return token?.id || null;
    } catch (error: any) {
      this.toastr.error(error?.message || 'Nao foi possivel tokenizar o cartao.');
      return null;
    }
  }

  private detectarBandeiraMercadoPago(numeroCartao: string): string {
    const numero = (numeroCartao || '').replace(/\D/g, '');

    if (/^4/.test(numero)) return 'visa';
    if (/^(5[1-5]|2[2-7])/.test(numero)) return 'master';
    if (/^3[47]/.test(numero)) return 'amex';
    if (/^((4011(78|79))|(431274)|(438935)|(451416)|(457393)|(45763[12])|(504175)|(5067)|(509)|(627780)|(636297)|(636368))/.test(numero)) return 'elo';
    if (/^(606282|3841)/.test(numero)) return 'hipercard';

    return 'visa';
  }

  private iniciarPollingStatus() {
    this.pararPollingStatus();
    this.statusPollingHandle = setInterval(() => this.verificarStatusCobranca(), 5000);
  }

  private pararPollingStatus() {
    if (this.statusPollingHandle) {
      clearInterval(this.statusPollingHandle);
      this.statusPollingHandle = null;
    }
  }

  private finalizarPagamentoConfirmado(formaPagamentoNome: string) {
    this.pararPollingStatus();

    if (this.cobrancaSelecionada) {
      this.cobrancaSelecionada.status = 'Pago';
      this.cobrancaSelecionada.formaPagamentoNome = formaPagamentoNome;
      this.cobrancaSelecionada.dataPagamento = new Date().toISOString();
    }

    this.modalRef?.hide();
    this.carregarCobrancasAcademia();
  }

  private resetPagamentoState() {
    this.pararPollingStatus();
    this.qrCodePix = '';
    this.pixPayload = '';
    this.pixGerado = false;
    this.pagamentoMensagem = '';
    this.pagamentoStatus = '';
    this.pagamentoVerificacaoAutomatica = false;
    this.cartao = {
      numero: '',
      nome: '',
      validade: '',
      cvv: '',
      cpf: '',
    };
  }

  private getMensagemStatus(status: string, metodo: string) {
    if (status === 'Pago') {
      return `Pagamento ${metodo || 'da cobranca'} confirmado com sucesso.`;
    }

    if (status === 'EmAnalise') {
      return 'Pagamento em analise. Aguarde a confirmacao.';
    }

    if (status === 'Pendente') {
      return 'Pagamento pendente. Aguarde a confirmacao.';
    }

    return 'Pagamento recusado.';
  }

  private formValido(): boolean {
    if (this.valor <= 0) {
      this.toastr.warning('Informe um valor maior que zero.');
      return false;
    }

    if (this.prazoPagamentoDias <= 0) {
      this.toastr.warning('Informe o prazo de pagamento em dias.');
      return false;
    }

    if (this.mesesUso <= 0) {
      this.toastr.warning('Informe os meses de uso.');
      return false;
    }

    if (!this.aceitaPix && !this.aceitaCartao) {
      this.toastr.warning('Habilite PIX, cartao ou ambos para a cobranca.');
      return false;
    }

    if (this.aceitaCartao && !this.mercadoPagoPublicKey.trim()) {
      this.toastr.warning('Informe a chave publica do Mercado Pago para pagamento com cartao.');
      return false;
    }

    if (!this.editarId && !this.mercadoPagoAccessToken.trim()) {
      this.toastr.warning('Informe o Access Token do Mercado Pago para receber a cobranca.');
      return false;
    }

    return true;
  }

  private resetForm() {
    this.valor = 0;
    this.prazoPagamentoDias = 0;
    this.mesesUso = 0;
    this.descricao = '';
    this.ativo = true;
    this.aceitaPix = true;
    this.aceitaCartao = true;
    this.mercadoPagoPublicKey = '';
    this.mercadoPagoAccessToken = '';
    this.mensalidadeSelecionada = null;
  }
}
