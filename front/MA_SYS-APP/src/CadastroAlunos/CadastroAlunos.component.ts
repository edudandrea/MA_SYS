import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { AlunosService } from '../Services/AlunosService/Alunosservice';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { CadastroAlunosService } from '../Services/CadastroAlunos/CadastroAlunos.service';
import { CadastroAlunosContext } from '../Services/CadastroAlunos/CadastroAlunos.service';
import * as QRCode from 'qrcode';
import { PagamentosService } from '../Services/PagamentosService/Pagamentos.service';

@Component({
  selector: 'app-CadastroAlunos',
  templateUrl: './CadastroAlunos.component.html',
  styleUrls: ['./CadastroAlunos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class CadastroAlunosComponent implements OnInit {
  cpfBusca: string = '';
  emailBusca: string = '';
  nome: string = '';
  telefone: string = '';
  dataNascimento: string = '';
  mostrarFormulario: boolean = false;

  planoId: number = 0;
  planos: any[] = [];
  planoSelecionado: any = null;

  formaPagamentoId: number = 0;
  formasPagamento: any[] = [];
  formaPagamentoSelecionada: any = null;

  isPix: boolean = false;
  showQrCode: boolean = false;
  qrCodePix: string = '';
  pixPayload: string = '';

  aluno: any = null;
  alunoEncontrado: boolean = false;
  mostrarCadastro: boolean = false;

  numeroCartao: string = '';
  validadeCartao: string = '';
  cvvCartao: string = '';

  isCartao: boolean = false;
  bandeiraCartao: string = '';

  constructor(
    private modalService: BsModalService,
    private alunoService: AlunosService,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cadastroAlunosService: CadastroAlunosService,
    private context: CadastroAlunosContext,
    private pgService: PagamentosService,
  ) {}

  ngOnInit() {
    this.context.slug = this.route.snapshot.paramMap.get('academia')!;
    console.log('SLUG:', this.context.slug);
  }

  pesquisarAlunos() {
    if (!this.cpfBusca || !this.emailBusca) {
      this.toastr.warning('Informe CPF e Email');
      return;
    }

    this.spinner.show();

    this.cadastroAlunosService.getByCpfEmail(this.cpfBusca, this.emailBusca).subscribe({
      next: (res) => {
        this.spinner.hide();

        this.aluno = res;
        this.carregarFormaPgtos();
        console.log('RETORNO API:', res);
        this.alunoEncontrado = true;
        this.mostrarCadastro = false;
      },
      error: (err) => {
        this.spinner.hide();

        if (err.status === 404) {
          this.mostrarCadastro = true;
          this.alunoEncontrado = false;
        } else {
          this.toastr.error('Erro ao buscar aluno');
        }
      },
    });
  }

  onPlanoChange() {
    console.log('ANTES FIND:', this.planoId, typeof this.planoId);

    const plano = this.planos.find((p) => p.id == this.planoId);

    console.log('RESULTADO FIND:', plano);

    this.planoSelecionado = plano;

    console.log('DEPOIS:', this.planoSelecionado);

    this.showQrCode = false; // 🔥 OCULTA O QR CODE AO MUDAR DE PLANO
    this.qrCodePix = ''; // 🔥 LIMPA O QR CODE ANTERIOR
  }

  onFormaPagamentoChange() {
    console.log('Forma de pagamento selecionada:', this.formaPagamentoId);
    this.formaPagamentoSelecionada = this.formasPagamento.find(
      (f) => f.id == this.formaPagamentoId,
    );
    const nome = this.formaPagamentoSelecionada?.nome?.toLowerCase() || '';

    this.isPix = this.formaPagamentoSelecionada?.nome?.toLowerCase() === 'pix';

    this.isCartao = nome.includes('crédito') || nome.includes('debito');

    this.showQrCode = false; // 🔥 OCULTA O QR CODE AO MUDAR DE FORMA DE PAGAMENTO
    this.qrCodePix = ''; // 🔥 LIMPA O QR CODE ANTERIOR
    this.numeroCartao = ''; // 🔥 LIMPA O NÚMERO DO CARTÃO ANTERIOR
    this.validadeCartao = ''; // 🔥 LIMPA A VALIDADE DO CARTÃO ANTERIOR
    this.cvvCartao = ''; // 🔥 LIMPA O CVV DO CARTÃO ANTERIOR
  }

  gerarPix() {
    const valor = this.planoSelecionado?.valor || 0;

    this.pgService.gerarPix(valor).subscribe((res) => {
      this.pixPayload = res.payload;
      this.gerarCodePix(res.payload);
    });
  }

  gerarCodePix(payload: string) {
    QRCode.toDataURL(payload).then((url) => {
      setTimeout(() => {
        this.qrCodePix = url;
        this.showQrCode = true;
      }, 0);
    });
  }

  copiarPix() {
    if (!this.pixPayload) return;

    navigator.clipboard.writeText(this.pixPayload);

    this.toastr.success('PIX copiado! Cole no app do seu banco.');
  }

  carregarFormaPgtos() {
    this.spinner.show();

    this.pgService.getFormaPagamentos().subscribe({
      next: (res) => {
        this.spinner.hide();

        console.log('Forma de pagamento recebidas:', res);

        this.formasPagamento = res.map((p) => ({
          ...p,
          menuAberto: false,
        }));
        //this.cd.markForCheck();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar forma de pagamento', 'Erro');
      },
    });
  }
}
