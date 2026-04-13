import { ChangeDetectorRef, Component, HostListener, OnInit, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import { Professores, ProfessorService } from '../Services/ProfessorService/Professor.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Academias, AcademiasService } from '../Services/AcademiaService/Academias.service';
import { Modalidades, ModalidadesService } from '../Services/ModalidadeService/Modalidades.service';

@Component({
  selector: 'app-Professores',
  templateUrl: './Professores.component.html',
  styleUrls: ['./Professores.component.css'],
  imports: [CommonModule, FormsModule],
})
export class ProfessoresComponent implements OnInit {
  modalRef?: BsModalRef;
  professorId: number = 0;
  nome: string = '';
  telefone: string = '';
  nomeAcademia: string = '';
  nomeModalidade: string = '';
  ativo: boolean = true;
  email: string = '';
  graduacao: string = '';
  isAdmin = false;

  academiaMap = new Map<number, string>();

  modalidadeMap = new Map<number, string>();

  modalidadeId: number = 0;

  totalAlunos: number = 0;

  academiaId: number = 0;

  editarId: number | null = null;

  professores: (Professores & { menuAberto?: boolean; academiaNome?: string })[] = [];

  academias: (Academias & { menuAberto?: boolean })[] = [];

  modalidades: (Modalidades & { menuAberto?: boolean })[] = [];

  modalidadesFiltradas: Modalidades[] = [];

  totalModalidades: Modalidades[] = [];

  academiaSelecionada: any;

  constructor(
    private modalService: BsModalService,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private prof: ProfessorService,
    private cd: ChangeDetectorRef,
    private acad: AcademiasService,
    private mod: ModalidadesService,
  ) {}

  ngOnInit() {
    this.getUserRole();    
  }

  @HostListener('document:click', ['$event'])
  fecharMenu(event: any) {
    const clicouMenu = event.target.closest('.card-menu');

    if (!clicouMenu) {
      this.professores.forEach((m: any) => (m.menuAberto = false));
    }
  }

  //------- GETS ----------

  getUserRole() {
    const role = localStorage.getItem('role');
    const academiaId = localStorage.getItem('academiaId');
    this.isAdmin = role === 'Admin';
    if (!this.isAdmin && academiaId) {
      this.academiaId = +academiaId;

      this.filtrarModalidades();
    }
    this.carregarAcademias();
    this.carregarModalidades();
  }

  getInicial(nome: string): string {
    return nome ? nome.charAt(0).toUpperCase() : '?';
  }

  carregarModalidades() {
    this.spinner.show();
    this.mod.getModalidades().subscribe({
      next: (res) => {
        console.log('Modalidades recebidas:', res);

        this.spinner.hide();

        this.totalModalidades = res;
        this.modalidadeMap = new Map(
          res.map(m => [m.id, m.nomeModalidade])
        );

        this.filtrarModalidades();

        this.cd.markForCheck();
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar modalidades');
        this.spinner.hide();
      },
    });
  }

  carregarAcademias() {
    this.acad.getAcademias().subscribe({
      next: (res) => {
        console.log('Academias recebidas:', res);
        this.spinner.hide();
        this.academias = res;

        this.academiaMap = new Map(
          res.map(a => [a.id, a.nome])
        );       

        this.carregarModalidades();

        this.carregarProfessores();

        this.cd.markForCheck();
      },

      error: (err) => {
        console.error(err);
        this.toastr.error('Erro ao carregar Academias');
      },
    });
  }

  carregarProfessores() {
    this.spinner.show();

    this.prof.getProfessores().subscribe({
      next: (res) => {
        this.spinner.hide();        

        console.log('Professores recebidos:', res);

        this.professores = res.map(p => ({
          ...p,
          menuAberto: false,
          academiaNome: this.academiaMap.get(p.academiaId) || 'Sem Academia',
          nomeModalidade: this.modalidadeMap.get(p.modalidadeId) || 'Sem Modalidade'
        }));
        this.cd.markForCheck();
      },

      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar professor', 'Erro');
      },
    });
  }

  filtrarModalidades() {
    this.modalidadesFiltradas = this.totalModalidades.filter(
      (m) => +m.academiaId === +this.academiaId,
    );
  }

  onAcademiaChange() {
    this.modalidadeId = 0;
    this.filtrarModalidades();
  }

  //------- TOOGLES ----------

  toggleMenu(modalidade: any, event: Event) {
    event.stopPropagation();

    this.professores.forEach((m: any) => {
      if (m !== modalidade) {
        m.menuAberto = false;
      }
    });

    modalidade.menuAberto = !modalidade.menuAberto;

    this.professores = [...this.professores];
  }

  toggleStatus(professor: Professores) {
    const novoStatus = !professor.ativo;

    this.prof.atualizarStatus(professor.id, novoStatus).subscribe({
      next: () => {
        professor.ativo = novoStatus;

        this.professores = [...this.professores];

        this.cd.markForCheck();
        this.carregarProfessores();

        this.toastr.success(`Professor ${novoStatus ? 'ativado' : 'desativado'}`);
      },

      error: () => {
        this.toastr.error('Erro ao atualizar professor');
      },
    });
  }

  // Abre o modal para cadastrar um novo professor

  openModalNovoProfessor(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, {
      class: 'modal-md modal-dialog-centered',
    });
  }
  //------- POST ----------

  cadastrarNovoProfessor() {
    this.spinner.show();

    const professor = {
      nome: this.nome,
      graduacao: this.graduacao,
      email: this.email,
      telefone: this.telefone,
      academiaId: this.academiaId,
      modalidadeId: this.modalidadeId,
    };

    console.group('📤 NOVO PROFESSOR');
    console.log(JSON.stringify(professor, null, 2));
    console.groupEnd();

    this.prof.novoProfessor(professor).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.toastr.success('Professor cadastrado!', 'Sucesso');

        this.nome = '';

        this.modalRef?.hide();

        this.carregarProfessores();
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao salvar professor', 'Erro');
      },
    });
  }

  // ---------PUT ----------

  editarProfessor(prof: Professores) {
    this.editarId = prof.id;
    this.nome= prof.nome;
    this.graduacao = prof.graduacao;
    this.email = prof.email;
    this.telefone = prof.telefone
    prof.menuAberto = false;
  }

  cancelarEdicao() {
    this.editarId = null;
  }

  salvarEdicao(prof: Professores) {
    const payload = {
      id: prof.id,
      nome: this.nome,
      graduacao: this.graduacao,
      telefone: this.telefone,
      email: this.email,
      ativo: prof.ativo,
    };

    this.prof.atualizarProfessor(payload).subscribe({
      next: () => {
        prof.nome = this.nome;
        prof.graduacao = this.graduacao;
        prof.telefone = this.telefone,
        prof.email = this.email

        this.editarId = null;

        this.carregarProfessores();

        this.toastr.success('Modalidade atualizada');
      },
    });
  }

  //------- DELETE ----------

  excluirProfessor(professorId: number): void {
    if (confirm('Deseja realmente excluir essa professor?')) {
      this.spinner.show();
      this.prof.excluirProfessor(professorId).subscribe({
        next: () => {
          this.toastr.success('Professor excluída com sucesso!', 'Sucesso');
          this.spinner.hide();
          this.carregarProfessores();
        },
        error: (err) => {
          console.error('Erro ao excluir professor', err);
          this.toastr.error('Erro ao excluir professor!', 'Erro');
          this.spinner.hide();
        },
      });
    }
  }
}
