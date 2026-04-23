/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { CadastroAlunosService } from './CadastroAlunos.service';

describe('Service: CadastroAlunos', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CadastroAlunosService]
    });
  });

  it('should ...', inject([CadastroAlunosService], (service: CadastroAlunosService) => {
    expect(service).toBeTruthy();
  }));
});
