# Checklist de Lancamento Comercial

## Seguranca e acesso

- [ ] Revisar todas as rotas para garantir escopo por `Academia`, `Admin` e `SuperAdmin`.
- [ ] Exigir senha forte e politica minima de renovacao.
- [ ] Registrar auditoria de login, troca de senha e exclusoes.
- [ ] Validar tentativas de login e aplicar bloqueio temporario por repeticao.

## Operacao financeira

- [ ] Confirmar conciliacao entre pagamentos de alunos, pagamentos de academias e fluxo de caixa.
- [ ] Implementar estorno, cancelamento e historico de alteracoes financeiras.
- [ ] Disponibilizar exportacao CSV/PDF para financeiro e dashboard.
- [ ] Configurar alertas para inadimplencia e falhas em cobranca.

## Produto e dados

- [ ] Validar onboarding completo de academia, usuario e plano sem apoio manual.
- [ ] Padronizar logo, identidade visual e dados da academia em todo o sistema.
- [ ] Revisar campos obrigatorios e mensagens de erro para evitar cadastro inconsistente.
- [ ] Criar backup automatico e plano de restauracao do banco.

## Qualidade e entrega

- [ ] Subir pipeline com build de front e back em toda alteracao.
- [ ] Cobrir login, usuarios, pagamentos e fluxo de caixa com testes automatizados.
- [ ] Publicar ambiente de homologacao com base mascarada para demonstracao.
- [ ] Monitorar logs de erro, latencia e eventos criticos apos o go-live.
