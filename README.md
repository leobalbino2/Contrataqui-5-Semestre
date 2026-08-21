## ContratAqui - 5º Semestre

# Marketplace Digital de Serviços

> Um marketplace digital de serviços que facilita a conexão entre clientes e profissionais prestadores de serviços.

---

## Visão do Produto

Um marketplace digital de serviços que facilita a conexão entre clientes e profissionais prestadores de serviços.

---

## Backlog do Produto

### Prototipação

- Prototipação das telas do aplicativo mobile.

### Desenvolvimento do Aplicativo Mobile

#### Telas e Navegação

- Desenvolvimento das telas do aplicativo.
- Menu de categorias e serviços.

#### Perfis

- Perfil do profissional com informações e avaliações.
- Perfil do usuário.

#### Autenticação e Cadastro

- Login e autenticação.
- Cadastro de clientes e profissionais.

#### Dashboards

- Dashboards para os diferentes tipos de usuários.

#### Ciclo de Contratação de Serviços

- Solicitação e contratação do serviço.
- Pagamento.
- Acompanhamento do status do serviço.
- Histórico de serviços contratados.

---

## Documentação

- Atualização da documentação do sistema.
- Documentação das novas funcionalidades do aplicativo mobile.
- Atualização da arquitetura da solução.

---

## Arquitetura da Solução

```text
Aplicativo Mobile (Frontend)
            │
            ▼
API / Backend (Node e Express)
            │
            ├── Autenticação
            ├── Usuários
            ├── Serviços
            ├── Avaliações
            └── Pagamentos
            │
            ▼
Banco de Dados (MySQL)
            │
            ├── Usuários
            ├── Serviços
            ├── Contratos
            └── Avaliações


