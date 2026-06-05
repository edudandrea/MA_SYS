import { ChangeDetectorRef, Component, OnInit, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlunosService, Alunos } from '../Services/AlunosService/Alunosservice';
import { FormsModule } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { ElementRef, ViewChild } from '@angular/core';
import { Modalidades, ModalidadesService } from '../Services/ModalidadeService/Modalidades.service';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';
import { Planos, PlanosService } from '../Services/Planos/Planos.service';
import { MatriculasService } from '../Services/MatriculasService/Matriculas.service';
import { Pagamentos, PagamentosService } from '../Services/PagamentosService/Pagamentos.service';

export enum TabsCadastroAluno {
  Pesquisa = 'PESQUISA',
  Cadastro = 'CADASTRO',
  AulaExperimental = 'AULA_EXPERIMENTAL',
}

@Component({
  selector: 'app-cadastro-alunos',
  standalone: true,
  templateUrl: './Alunos.component.html',
  styleUrls: ['./Alunos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class AlunosComponent implements OnInit {
  modalRef?: BsModalRef;
  @ViewChild('inputNomeModal') inputNomeModal!: ElementRef;
  @ViewChild('templateConfirmarMatricula') templateConfirmarMatricula!: TemplateRef<any>;
  @ViewChild('templateNovaMatriculaAluno') templateNovaMatriculaAluno!: TemplateRef<any>;

  activeTab: TabsCadastroAluno = TabsCadastroAluno.Pesquisa;
  idAluno: number = 0;
  nome: string = '';
  cpf: string = '';
  endereco: string = '';
  bairro: string = '';
  cidade: string = '';
  cep: string = '';
  estado: string = '';
  graduacao: string = '';
  modalidadeId: number = 0;
  planoId: number = 0;
  telefone: string = '';
  email: string = '';
  modoEdicao = false;
  dataNascimento: string = '';
  dataCadastro: string = '';
  modalidades: Modalidades[] = [];
  planos: Planos[] = [];
  formasPagamento: Pagamentos[] = [];
  redeSocial: string = '';
  ativo: boolean = true;
  obs: string = '';
  mensalidadeStatus = '';
  dataVencimentoMensalidade = '';
  diasParaVencimento: number | null = null;

  totalAlunos: number = 0;

  TabsCadastroAluno = TabsCadastroAluno;

  filtroAlunos = { }as any;
  novoAlunoForm = this.createNovoAlunoForm();
  aulaExperimentalForm = this.createAulaExperimentalForm();
  novaMatriculaForm = this.createNovaMatriculaForm();

  tabs = [
    {
      id: TabsCadastroAluno.Pesquisa,
      label: 'Pesquisa de Alunos',
      icon: 'bi bi-search',
    },
    {
      id: TabsCadastroAluno.Cadastro,
      label: 'Cadastro de Alunos',
      icon: 'bi bi-person-vcard',
    },
    {
      id: TabsCadastroAluno.AulaExperimental,
      label: 'Aula experimental',
      icon: 'bi bi-calendar-plus',
    },
  ];

  mostrarListaAlunos = false;

  alunosFiltrados: Alunos[] = [];
  alunosResultado: (Alunos & { academiaNome?: string })[] = [];

  selectedAluno?: Alunos & { academiaNome?: string };
  alunoParaNovaMatricula?: Alunos;

  animacaoStatus: 'pulse' | 'shake' | null = null;

  academias: Academias[] = [];

  academiaMap = new Map<number, string>();

  private debounceTimer: any;

  constructor(
    private toastr: ToastrService,
    private alunoService: AlunosService,
    private modalService: BsModalService,
    private spinner: NgxSpinnerService,
    private modalidadesService: ModalidadesService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService,
    private planosService: PlanosService,
    private matriculasService: MatriculasService,
    private pagamentosService: PagamentosService,
  ) {}

  ngOnInit() {
    this.carregarAcademias();
    this.carregarModalidades();
  }

  setActiveTab(tab: TabsCadastroAluno) {
    this.activeTab = tab;
    if (tab === TabsCadastroAluno.Pesquisa) {
      this.resetFiltro();
    }
    if (tab === TabsCadastroAluno.AulaExperimental) {
      this.resetAulaExperimentalForm();
      this.carregarModalidades();
    }
  }

  // Métodos para pesquisar alunos

  pesquisarAlunosAutocomplete(nome: string): void {
    clearTimeout(this.debounceTimer);

    const termo = String(nome ?? '').trim();

    if (termo.length < 2) {
      this.alunosFiltrados = [];
      this.mostrarListaAlunos = false;
      return;
    }

    this.debounceTimer = setTimeout(() => {
      const filtro: any = { nome: termo };

      this.alunoService.pesquisarAlunos(filtro).subscribe({
        next: (res) => {
          this.alunosFiltrados = res ?? [];
          this.mostrarListaAlunos = this.alunosFiltrados.length > 0;
        },
        error: () => {
          this.alunosFiltrados = [];
          this.mostrarListaAlunos = false;
        },
      });
    }, 300);
  }

  onPesquisarClick(): void {
    const filtro: any = {};
    if (this.filtroAlunos.id) filtro.id = Number(this.filtroAlunos.id);
    if (this.filtroAlunos.nome) filtro.nome = this.filtroAlunos.nome;
    if (this.filtroAlunos.CPF) filtro.CPF = this.filtroAlunos.CPF;
    if (this.filtroAlunos.graduacao) filtro.graduacao = this.filtroAlunos.graduacao;

    this.pesquisarAlunos(filtro);
  }

  fecharListaAlunosComDelay() {
    setTimeout(() => {
      this.mostrarListaAlunos = false;
    }, 0);
  }

  selecionarAlunoDaLista(aluno: Alunos) {
    if (!aluno) return;
    this.filtroAlunos.nome = aluno.nome;
    this.filtroAlunos.CPF = aluno.cpf;
    this.mostrarListaAlunos = false;
    this.alunosFiltrados = [];

    this.setAlunoAtual(aluno);
    this.onPesquisarClick();
  }

  abrirAluno(aluno: Alunos) {
    this.setAlunoAtual(aluno);
    this.activeTab = TabsCadastroAluno.Cadastro;
    this.modoEdicao = false;
  }

  onInputNome(event: any) {
    const valor = event.target.value;

    this.filtroAlunos.nome = valor;

    clearTimeout(this.debounceTimer);

    this.debounceTimer = setTimeout(() => {
      if (!valor || valor.length < 2) {
        this.mostrarListaAlunos = false;
        this.alunosFiltrados = [];
        return;
      }

      this.pesquisarAlunosAutocomplete(valor);
    }, 100);
  }

  abrirAutocomplete() {
    if (this.alunosFiltrados.length > 0) {
      this.mostrarListaAlunos = true;
    }
  }

  setAlunoAtual(aluno: Alunos) {
    console.log('🔍 Pesquisando alunos com filtro:', aluno);
    if (!aluno) return;

    this.selectedAluno = {
      ...aluno,
      academiaNome: this.academiaMap.get(aluno.academiaId) || 'Sem Academia',
    };

    this.idAluno = aluno.id;
    this.nome = aluno.nome;
    this.cpf = aluno.cpf;
    this.endereco = aluno.endereco ?? '';
    this.graduacao = aluno.graduacao;
    this.modalidadeId = aluno.modalidadeId;
    this.telefone = aluno.telefone;
    this.email = aluno.email;
    this.redeSocial = aluno.redeSocial ?? '';
    this.dataNascimento = aluno.dataNascimento?.split('T')[0] ?? '';
    this.bairro = aluno.bairro ?? '';
    this.cidade = aluno.cidade ?? '';
    this.cep = aluno.cep ?? '';
    this.estado = aluno.estado ?? '';
    this.dataCadastro = aluno.dataCadastro?.substring(0, 10);
    this.ativo = aluno.ativo;
    this.planoId = Number(aluno.planoId);
    this.obs = aluno.obs ?? '';
    this.mensalidadeStatus = aluno.mensalidadeStatus ?? 'Em dia';
    this.dataVencimentoMensalidade = aluno.dataVencimentoMensalidade?.substring(0, 10) ?? '';
    this.diasParaVencimento = aluno.diasParaVencimento ?? null;
  }

  pesquisarAlunos(filtro: any): void {
    this.spinner.show();
    this.alunoService.pesquisarAlunos(filtro).subscribe({
      next: (res) => {
        this.spinner.hide();
        if (!res?.length) {
          this.alunosResultado = [];
          this.toastr.info('Nenhum aluno encontrado.');
          return;
        }

        this.alunosResultado = res.map((aluno) => ({
          ...aluno,
          academiaNome: this.academiaMap.get(aluno.academiaId) || 'Sem Academia',
        }));

        if (this.alunosResultado.length === 1) {
          this.setAlunoAtual(this.alunosResultado[0]);
        }
      },
      error: () => {
        this.spinner.hide();
        this.toastr.error('Erro ao buscar alunos');
      },
    });
  }

  resetFiltro() {
    this.filtroAlunos = {
      id: '',
      nome: '',
      CPF: '',
      graduacao: '',
      modalidadeId: 0,
      planoId: 0,
    };
  }

  // Métodos para carregar dados de apoio (modalidades, academias, planos)

  

  

  carregarModalidades() {
    this.modalidadesService.getModalidades().subscribe({
      next: (res) => {
        this.modalidades = (res ?? []).filter((modalidade) => modalidade.ativo !== false);
        this.cd.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.modalidades = [];
        this.toastr.error('Erro ao carregar modalidades');
      },
    });
  }

  carregarAcademias() {
    this.spinner.show();
    this.acad.getAcademias().subscribe({
      next: (res) => {
        console.log('Academias recebidas:', res);
        this.spinner.hide();
        this.academias = res;

        this.academiaMap.clear();
        this.academias.forEach((a) => this.academiaMap.set(a.id, a.nome));

        this.cd.markForCheck(); // força Angular atualizar
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
      },
    });
  }

  cadastroNovoAluno() {
    if (!this.novoAlunoForm.nome?.trim()) {
      this.toastr.warning('Informe o nome do aluno');
      return;
    }

    if (this.novoAlunoForm.modalidadeId <= 0) {
      this.toastr.warning('Selecione uma modalidade');
      return;
    }

    this.spinner.show();

    const aluno = {
      nome: this.novoAlunoForm.nome,
      cpf: this.novoAlunoForm.cpf,
      graduacao: this.novoAlunoForm.graduacao,
      modalidadeId: this.novoAlunoForm.modalidadeId,
      telefone: this.novoAlunoForm.telefone,
      email: this.novoAlunoForm.email,
      redeSocial: this.novoAlunoForm.redeSocial,
      dataCadastro: this.dataCadastro || new Date().toISOString(),
    };

    console.group('📤 NOVO ALUNO');
    console.log(JSON.stringify(aluno, null, 2));
    console.groupEnd();

    this.alunoService.novoAluno(aluno).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Aluno cadastrado!', 'Sucesso');

        this.setAlunoAtual(res);

        this.activeTab = TabsCadastroAluno.Cadastro;
        this.resetFiltro();
        this.resetNovoAlunoForm();
        this.fecharModal();
        this.abrirConfirmacaoMatricula(res);
        
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar aluno', 'Erro');
      },
    });
  }

  ativarEdicao() {
    if (!this.selectedAluno) {
      this.toastr.warning('Pesquise um aluno antes de editar.');

      return;
    }

    this.modoEdicao = true;
  }

  cancelarEdicao() {
    this.modoEdicao = false;

    if (this.selectedAluno) {
      this.setAlunoAtual(this.selectedAluno);
    }
  }
  confirmarCancelarEdicao() {
    this.modalRef?.hide();
    this.cancelarEdicao();
  }

  openModalCancelarEdicao(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-sm modal-dialog-centered',
    });
  }
  // Métodos para cadastro/edição de alunos - MODAL
  fecharModal() {
    this.modalRef?.hide();
  }

  cadastrarAulaExperimental() {
    if (!this.aulaExperimentalForm.nome?.trim()) {
      this.toastr.warning('Informe o nome do aluno');
      return;
    }

    if (this.aulaExperimentalForm.modalidadeId <= 0) {
      this.toastr.warning('Selecione uma modalidade');
      return;
    }

    this.spinner.show();

    const modalidade = this.modalidades.find((item) => item.id === this.aulaExperimentalForm.modalidadeId);
    const observacao = [
      'Aula experimental',
      this.aulaExperimentalForm.dataAula ? `Data: ${this.aulaExperimentalForm.dataAula}` : '',
      modalidade?.nomeModalidade ? `Modalidade: ${modalidade.nomeModalidade}` : '',
      this.aulaExperimentalForm.observacoes?.trim() || '',
    ].filter(Boolean).join(' | ');

    this.alunoService.novoAluno({
      nome: this.aulaExperimentalForm.nome.trim(),
      telefone: this.aulaExperimentalForm.telefone,
      email: this.aulaExperimentalForm.email,
      modalidadeId: this.aulaExperimentalForm.modalidadeId,
      graduacao: 'Aula experimental',
      obs: observacao,
      dataCadastro: new Date().toISOString(),
    } as any).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Aula experimental cadastrada!', 'Sucesso');
        this.setAlunoAtual(res);
        this.activeTab = TabsCadastroAluno.Cadastro;
        this.resetAulaExperimentalForm();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao cadastrar aula experimental', 'Erro');
      },
    });
  }

  abrirConfirmacaoMatricula(aluno: Alunos) {
    this.alunoParaNovaMatricula = aluno;

    setTimeout(() => {
      this.modalRef = this.modalService.show(this.templateConfirmarMatricula, {
        class: 'modal-sm modal-dialog-centered',
      });
    }, 200);
  }

  abrirModalMatriculaNovoAluno() {
    if (!this.alunoParaNovaMatricula) return;

    this.modalRef?.hide();
    this.novaMatriculaForm = this.createNovaMatriculaForm();
    this.carregarPlanos();
    this.carregarFormasPagamento();

    setTimeout(() => {
      this.modalRef = this.modalService.show(this.templateNovaMatriculaAluno, {
        class: 'modal-lg modal-dialog-centered',
      });
    }, 200);
  }

  recusarNovaMatricula() {
    this.modalRef?.hide();
    this.alunoParaNovaMatricula = undefined;
  }

  openModalNovoAluno(template: TemplateRef<any>) {
    this.resetNovoAlunoForm();
    this.carregarModalidades();
    this.modalRef = this.modalService.show(template, {
      class: 'modal-lg modal-dialog-centered',
    });
    
    setTimeout(() => {
      this.inputNomeModal?.nativeElement.focus();
    }, 200);
  }

  atualizarAluno() {
    if (!this.idAluno) {
      this.toastr.warning('Aluno não selecionado.', 'Atenção');
      return;
    }

    if (!this.modoEdicao) return;

    const payload: Partial<Alunos> = {
      nome: String(this.nome ?? '').trim(),
      cpf: this.cpf ?? undefined,
      graduacao: this.graduacao ?? undefined,
      modalidadeId: this.modalidadeId ?? undefined,
      telefone: String(this.telefone ?? '').replace(/\D/g, ''),
      email: this.email ?? undefined,
      endereco: this.endereco ?? undefined,
      bairro: this.bairro ?? undefined,
      cidade: this.cidade ?? undefined,
      estado: this.estado ?? undefined,
      cep: this.cep ?? undefined,
      redeSocial: this.redeSocial ?? undefined,
      dataNascimento: this.dataNascimento,
      ativo: this.ativo,
      planoId: this.planoId ?? undefined,
      obs: this.obs ?? undefined,
    };

    console.group('📤 ATUALIZAR ALUNO');
    console.log(JSON.stringify(payload, null, 2));
    console.groupEnd();

    this.spinner.show();

    this.alunoService.atualizarAluno(this.idAluno, payload).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Aluno atualizado com sucesso!', 'Sucesso');

        // 🔥 mantém estado atualizado
        this.modoEdicao = false;
        this.setAlunoAtual(res);
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao atualizar aluno', 'Erro');
      },
    });
  }

  alterarStatusAluno() {
    this.modoEdicao = false;

    const novoStatus = !this.ativo;

    this.alunoService.atualizarStatusAluno(this.idAluno, novoStatus).subscribe({
      next: () => {
        this.ativo = novoStatus;

        if (this.selectedAluno) {
          this.selectedAluno.ativo = novoStatus;
        }
        this.animacaoStatus = novoStatus ? 'pulse' : 'shake';

        setTimeout(() => {
          this.animacaoStatus = null;
        }, 600);

        this.cd.markForCheck();

        this.toastr.success(`Aluno ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao alterar status');
      },
    });
  }

  private createNovoAlunoForm() {
    return {
      nome: '',
      cpf: '',
      email: '',
      telefone: '',
      modalidadeId: 0,
      graduacao: '',
      redeSocial: '',
    };
    this.alunosResultado = [];
  }

  private resetNovoAlunoForm() {
    this.novoAlunoForm = this.createNovoAlunoForm();
  }

  private createAulaExperimentalForm() {
    return {
      nome: '',
      telefone: '',
      email: '',
      modalidadeId: 0,
      dataAula: new Date().toISOString().split('T')[0],
      observacoes: '',
    };
  }

  resetAulaExperimentalForm() {
    this.aulaExperimentalForm = this.createAulaExperimentalForm();
  }

  private createNovaMatriculaForm() {
    return {
      planoId: 0,
      formaPagamentoId: 0,
      dataInicio: new Date().toISOString().split('T')[0],
    };
  }

  carregarPlanos() {
    this.planosService.getPlanos().subscribe({
      next: (res) => {
        this.planos = (res ?? []).filter((plano) => plano.ativo !== false);
      },
      error: () => {
        this.planos = [];
        this.toastr.error('Erro ao carregar planos');
      },
    });
  }

  carregarFormasPagamento() {
    this.pagamentosService.getFormaPagamentos().subscribe({
      next: (res) => {
        this.formasPagamento = (res ?? []).filter((forma) => forma.ativo !== false);
      },
      error: () => {
        this.formasPagamento = [];
        this.toastr.error('Erro ao carregar formas de pagamento');
      },
    });
  }

  cadastrarMatriculaNovoAluno() {
    if (!this.alunoParaNovaMatricula?.id) {
      this.toastr.warning('Aluno nao selecionado');
      return;
    }

    if (!this.novaMatriculaForm.planoId) {
      this.toastr.warning('Selecione um plano');
      return;
    }

    if (!this.novaMatriculaForm.formaPagamentoId) {
      this.toastr.warning('Selecione a forma de pagamento');
      return;
    }

    this.spinner.show();
    this.matriculasService.novaMatricula({
      alunoId: this.alunoParaNovaMatricula.id,
      planoId: this.novaMatriculaForm.planoId,
      formaPgtoId: this.novaMatriculaForm.formaPagamentoId,
      dataInicio: this.novaMatriculaForm.dataInicio,
    } as any).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Matricula cadastrada!', 'Sucesso');
        this.modalRef?.hide();
        this.alunoParaNovaMatricula = undefined;
        this.novaMatriculaForm = this.createNovaMatriculaForm();
      },
      error: (err) => {
        this.spinner.hide();
        const message = err?.error?.message || 'Erro ao salvar matricula';
        this.toastr.error(message, 'Erro');
      },
    });
  }

  get mensalidadeStatusClasse(): string {
    const status = (this.mensalidadeStatus || '').toLowerCase();

    if (status === 'pago') return 'mensalidade-paga';
    if (status === 'pendente') return 'mensalidade-pendente';
    if (status === 'em atraso') return 'mensalidade-atraso';
    return 'mensalidade-em-dia';
  }
}
