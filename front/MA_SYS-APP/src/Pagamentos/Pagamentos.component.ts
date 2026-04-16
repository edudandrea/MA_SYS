import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Pagamentos, PagamentosService } from '../Services/PagamentosService/Pagamentos.service';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import * as QRCode from 'qrcode';
import { PixResponse } from '../Model/pix-response.model';

@Component({
  selector: 'app-Pagamentos',
  templateUrl: './Pagamentos.component.html',
  styleUrls: ['./Pagamentos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class PagamentosComponent implements OnInit {
  modalRef?: BsModalRef;
  pagamentos: any[] = [];
  nome: string = '';
  ativo: boolean = true;
  taxa: number = 0;
  parcelas: number = 0;
  dias: number = 0;
  qrCodePix: string = '';
  pixPayload: string = '';
  valor: number = 0;

  novoFormaPgto = {
    nome: '',
    ativo: true,
    taxa: 0,
    parcelas: 0,
    dias: 0,
  };

  editarId: number | null = null;

  constructor(
    private pgService: PagamentosService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cd: ChangeDetectorRef,
    private modalService: BsModalService,
  ) {}

  ngOnInit() {
    this.carregarFormaPgtos();
  }

  gerarQrCodePix(payload: string) {
    console.log('Gerando QR com payload:', payload);
    QRCode.toDataURL(payload)
      .then((url) => {
        this.qrCodePix = url;
        this.cd.detectChanges();
      })
      .catch((err) => {
        console.error('Erro ao gerar QRCode:', err);
      });
  }

  gerarPix() {
    if (!this.valor || this.valor <= 0) {
      this.toastr.warning('Informe um valor válido');
      return;
    }

    this.spinner.show();

    this.pgService.gerarPix(this.valor).subscribe({
      next: (res: PixResponse) => {
        this.spinner.hide();

        this.pixPayload = res.payload;

        this.gerarQrCodePix(res.payload);

        this.toastr.success('Pix gerado com sucesso!');
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao gerar Pix');
      },
    });
  }

  testarPix() {
    // valor fixo só pra teste
    this.valor = 50;

    this.gerarPix();
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  toggleMenu(modalidade: any, event: Event) {
    event.stopPropagation();

    this.pagamentos.forEach((m: any) => {
      if (m !== modalidade) {
        m.menuAberto = false;
      }
    });

    modalidade.menuAberto = !modalidade.menuAberto;

    this.pagamentos = [...this.pagamentos];
  }

  toggleStatus(pgtos: Pagamentos) {
    const novoStatus = !pgtos.ativo;

    this.pgService.atualizarStatus(pgtos.id, novoStatus).subscribe({
      next: () => {
        pgtos.ativo = novoStatus;

        this.pagamentos = [...this.pagamentos];

        this.cd.markForCheck();
        this.carregarFormaPgtos();

        this.toastr.success(`Pagamento ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao atualizar pagamento');
      },
    });
  }

  // ---------- MODAL NOVA FORMA DE PAGAMENTO ----------

  openModalNovoPgto(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }

  //---------- GETS ----------

  carregarFormaPgtos() {
    this.spinner.show();

    this.pgService.getFormaPagamentos().subscribe({
      next: (res) => {
        this.spinner.hide();

        console.log('Forma de pagamento recebidas:', res);

        this.pagamentos = res.map((p) => ({
          ...p,
          menuAberto: false,
        }));
        this.cd.markForCheck();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar forma de pagamento', 'Erro');
      },
    });
  }

  // ---------- POST ----------
  cadastrarNovaFormaPgto() {
    this.spinner.show();

    const formaPgto = {
      nome: this.nome,
      ativo: this.ativo,
      taxa: this.taxa,
      parcelas: this.parcelas,
      dias: this.dias,
    };

    console.group('📤 NOVA FORMA DE PAGAMENTO');
    console.log(JSON.stringify(formaPgto, null, 2));
    console.groupEnd();

    this.pgService.novaFormaPgto(formaPgto).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Forma de pagamento cadastrada!', 'Sucesso');

        this.novoFormaPgto.nome = '';

        this.modalRef?.hide();
        this.carregarFormaPgtos();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar Forma de Pagamento', 'Erro');
      },
    });
  }
}
