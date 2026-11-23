# Contrataqui - 5º semestre
 
## Histórico de versões
 
| Data | Versão | Descrição | Autores |
|------|--------|-----------|---------|
| 25/08/2026 | 1.0 | Criação do Plano de Projeto Ágil de Software para o aplicativo mobile Contrataqui. | Leonardo Balbino, Nicoly Whitehead Maria Bonani |
| 23/09/2026 | 1.1 | Atualização do agora Projeto do Aplicativo mobile Contrataqui. | Leonardo Balbino, Nicoly Whitehead Maria Bonani |
| 04/11/2026 | 1.2 | Atualização do backlog do produto e dos épicos (realizado, em execução e planejado) e inclusão da arquitetura da camada de dados do aplicativo. | Leonardo Balbino, Nicoly Whitehead Maria Bonani |
 
## 1. Introdução
 
Este documento apresenta o Plano de Projeto Ágil de Software do aplicativo mobile Contrataqui, desenvolvido no contexto do Projeto Integrador do 5º semestre do curso de Desenvolvimento de Software Multiplataforma. O documento tem como finalidade formalizar a iniciação do projeto, apresentando o contexto do negócio, a visão do produto, os objetivos do desenvolvimento, a arquitetura da solução, as tecnologias previstas, o escopo, os papéis da equipe, o backlog do produto, o planejamento das sprints e os principais riscos do projeto.
 
O Contrataqui será um aplicativo mobile voltado à intermediação de serviços. A solução permitirá que clientes encontrem profissionais prestadores de serviços, visualizem informações sobre seus perfis, consultem serviços oferecidos, solicitem e contratem serviços, realizem pagamentos e acompanhem o andamento dos atendimentos. Para os profissionais, o aplicativo permitirá a divulgação de seus serviços, o recebimento de solicitações, o gerenciamento de contratos e o acompanhamento do histórico de serviços realizados.
 
O projeto tem como estudo de caso a empresa Estrutucasa, que atua no setor de estruturas metálicas e registrada como Microempreendedor Individual. A empresa representa um contexto real de prestação de serviços e será utilizada como referência para validar se as funcionalidades propostas atendem às necessidades de profissionais autônomos e pequenos negócios.
 
### Motivação para o desenvolvimento do app
 
A versão web do Contrataqui, desenvolvida no semestre passado, exige acesso via computador ou navegador do celular, o que não é ideal para o público alvo do projeto. Profissionais autônomos passam a maior parte do tempo em serviços ou em deslocamento, e muitas vezes demoram a responder um orçamento simplesmente porque não têm uma forma prática de acompanhar isso pelo celular. Isso pode significar perder o serviço para outro profissional mais rápido.
 
Um aplicativo resolve esse problema ao permitir responder orçamentos, atualizar o status de um serviço e conversar com clientes direto do celular, com notificações e uma interface pensada para uso móvel. Além de atender a essa necessidade real, o desenvolvimento de um app nativo/híbrido é também o requisito principal do Projeto Integrador deste semestre.
 
### 1.1 Contexto do aplicativo móvel
 
O Contrataqui Mobile não é um aplicativo isolado: ele faz parte da plataforma Contrataqui, um marketplace digital de serviços que conecta clientes a profissionais prestadores de serviço. A plataforma já possui uma versão web, desenvolvida no 4º semestre, apoiada em uma API REST (Node.js/Express/TypeScript) que concentra as regras de negócio e se integra aos serviços de autenticação (JWT), upload de imagens (Cloudinary), chat em tempo real (Socket.io) e ao banco de dados MySQL, acessado via Prisma ORM.
 
Nesse ecossistema, o aplicativo mobile entra como um novo cliente da API, ao lado do frontend web existente, sem alterar o backend nem duplicar regras de negócio. Sua principal função em relação à plataforma é ampliar os canais de distribuição, levando a jornada essencial de clientes e profissionais ao celular, o dispositivo mais utilizado pelo público-alvo, especialmente pelos profissionais autônomos que passam a maior parte do tempo em obra ou em deslocamento.
 
### 1.2 Contexto de negócio
 
Nos últimos anos, a presença digital se tornou importante para profissionais autônomos e pequenas empresas que desejam divulgar seus serviços, conquistar clientes e organizar seus atendimentos. Contudo, muitos prestadores ainda dependem de processos informais, como conversas em aplicativos de mensagens, divulgação por indicação e registros manuais de orçamentos e contratos.
 
Essa forma de trabalho pode dificultar a organização das solicitações, o acompanhamento dos serviços, o armazenamento do histórico de atendimentos e a comunicação entre clientes e profissionais. Do lado dos clientes, também pode ser difícil encontrar prestadores confiáveis, comparar serviços, consultar avaliações e acompanhar uma contratação de maneira centralizada.
 
O Contrataqui foi proposto para reduzir essas dificuldades por meio de um marketplace digital de serviços. A plataforma reunirá clientes e profissionais em um único ambiente, permitindo que os prestadores apresentem seus serviços e que os clientes encontrem, solicitem, contratem e acompanhem atendimentos de forma mais organizada.
 
O modelo de negócio do Contrataqui foi definido no 4º semestre por meio de um Business Model Canvas, utilizando a empresa Estrutucasa (MEI atuante no setor de estruturas metálicas) como estudo de caso para validação das funcionalidades da plataforma em um contexto real de mercado. Esse mesmo modelo de negócio é mantido no 5º semestre; o app mobile não altera a proposta de valor, mas amplia os canais de distribuição da plataforma.
 
- **Proposta de valor:** Plataforma prática e acessível que conecta clientes a profissionais prestadores de serviço, permitindo busca, seleção e contratação de serviços com avaliações e maior segurança na escolha do profissional.
- **Segmento de clientes:** Pessoas que precisam de serviços domésticos ou reformas, proprietários de imóveis, pequenos negócios e profissionais autônomos da construção civil.
- **Relacionamento com clientes:** Atendimento por meio do aplicativo, notificações, avaliações, feedbacks e acompanhamento do status das solicitações e dos serviços contratados.
- **Parcerias-chave:** Profissionais autônomos, microempresas prestadoras de serviços, empresas do setor da construção civil e possíveis empresas parceiras.
- **Canais:** Aplicação web (existente) e, a partir deste semestre, aplicativo mobile, além de divulgação em redes sociais e instituições de ensino.
- **Fontes de receita:** Projeto inicialmente sem fins lucrativos. Como possibilidade futura, poderão ser utilizadas comissões por serviços, planos para profissionais ou destaque de perfis.
- **Atividades chave:** Desenvolver e manter o aplicativo, gerenciar usuários, serviços, contratos, avaliações e pagamentos, além de garantir segurança e boa experiência de uso.
- **Estrutura de custos:** Horas de desenvolvimento e manutenção do aplicativo e da API, serviços de infraestrutura e armazenamento (backend, banco de dados e Cloudinary) e divulgação em redes sociais e instituições de ensino.
### 1.3 Grupo de trabalho e repositório Git
 
**Integrantes**
 
- Leonardo Balbino;
- Nicoly Whitehead Maria Bonani.
**Repositório do app:** <https://github.com/leobalbino2/Contrataqui-5-Semestre>
 
## 2. Aplicativo para Dispositivo Móvel
 
O Contrataqui Mobile leva a plataforma Contrataqui para o celular de clientes e profissionais.
 
### 2.1 Visão do produto
 
O Contrataqui Mobile é a versão para smartphones da plataforma Contrataqui, permitindo que clientes busquem profissionais, solicitem orçamentos, agendem serviços e acompanhem o histórico de atendimentos, e que profissionais gerenciem seu perfil, respondam solicitações de orçamento, controlem sua agenda e conversem com clientes. Tudo isso a partir do celular, com uma experiência otimizada para uso rápido e em movimento.
 
Quando completamente pronto e integrado ao ecossistema, espera-se que o app mobile ofereça as mesmas funcionalidades centrais do site (busca de profissionais, orçamentos, agendamento, histórico e chat), com vantagens específicas do mobile: notificações push para novas solicitações e mensagens, acesso mais rápido a funcionalidades do dia a dia, e uma interface adaptada para telas pequenas e uso com uma mão.
 
**Problema que o app resolve.** Profissionais autônomos passam a maior parte do tempo em serviço ou em deslocamento e, sem uma forma prática de acompanhar solicitações pelo celular, demoram a responder orçamentos e podem perder o serviço para outro profissional mais rápido. Para os clientes, o app resolve a dificuldade de encontrar prestadores, solicitar, contratar e acompanhar atendimentos de maneira centralizada, de onde quer que estejam.
 
### 2.2 Objetivos do desenvolvimento do app
 
**Por que deve ser desenvolvido?**
 
A plataforma web já validou o modelo de negócio e a arquitetura de backend no 4º semestre; o próximo passo natural — e requisito do Projeto Integrador do 5º semestre — é levar essa solução ao dispositivo que o público-alvo (profissionais autônomos em campo, clientes em qualquer lugar) mais utiliza no dia a dia: o celular.
 
**Para que servirá?**
 
- Ampliar o alcance da plataforma para usuários que preferem ou só têm acesso via smartphone;
- Dar aos profissionais uma ferramenta ágil para gerenciar orçamentos e agendamentos fora do escritório/casa;
- Aplicar, em um contexto prático, os conceitos de desenvolvimento mobile do curso de DSM;
- Manter os dados sincronizados entre web e mobile por meio da mesma API, evitando sistemas paralelos.
### 2.3 Arquitetura básica
 
O app mobile funcionará como um novo cliente que consome a API REST já construída no 4º semestre, sem alterar o backend nem duplicar regras de negócio. A comunicação ocorre via HTTPS/JSON, reaproveitando os mesmos serviços de autenticação (JWT), upload de imagens (Cloudinary), chat em tempo real (Socket.io) e o banco de dados MySQL (via Prisma).
 
**Arquitetura Básica – App Mobile integrado ao ecossistema existente.**
 
![Preview][arqu]
 
**Resumo da arquitetura:**
 
1. App Mobile (novo) e Frontend Web (existente) são dois clientes independentes que consomem a mesma API REST.
2. A API REST (Node.js/Express/TypeScript) concentra toda a lógica de negócio e expõe endpoints comuns aos dois clientes.
3. A API se comunica com os serviços de autenticação, upload de imagens e chat em tempo real, e com o banco de dados MySQL via Prisma ORM.
4. Nenhum dado é armazenado localmente de forma definitiva no app. O dispositivo móvel é uma camada de apresentação e interação, mantendo o backend como única fonte de verdade.
### 2.4 Metas e Restrições da Arquitetura
 
| Item | Definição | Justificativa |
|------|-----------|---------------|
| Tipo de aplicativo | Híbrido (framework cross-platform) | A dupla não tem tempo hábil, dentro de um semestre, para manter duas bases de código nativas (Android/iOS); um framework híbrido permite entregar Android e iOS a partir de uma única base. |
| Tecnologia candidata | React ou Flutter, a definir | A equipe já domina React/JavaScript no frontend web, o que tornaria o React Native uma transição natural (reaproveitando componentes mentais e parte da lógica). O Flutter é a alternativa caso a disciplina de Desenvolvimento Mobile do 5º semestre adote Dart/Flutter como padrão da turma. |
| Comunicação com backend | HTTP/HTTPS consumindo a API REST existente | Evita duplicar regras de negócio; garante que dados de web e mobile fiquem sempre sincronizados. |
| Autenticação | JWT (reaproveitado do backend atual) | Já implementado e testado no 4º semestre; reduz retrabalho e risco técnico. |
| Banco de dados | MySQL via Prisma ORM (sem alterações de schema por parte do mobile) | O mobile é apenas mais um consumidor da API; qualquer mudança de schema é feita no backend, não no app. |
| Restrição de tempo | Um semestre letivo, dividido em 3 entregas (EP2, EP3, EF) | Define a necessidade de priorizar um escopo mínimo viável (MVP) em vez de replicar 100% das funcionalidades do site já na primeira entrega. |
 
### 2.5 Funcionalidade Principal & Jornada do Usuário
 
A principal funcionalidade do app é a contratação de serviços: o cliente busca profissionais, visualiza seus perfis e solicita um orçamento; o profissional visualiza e responde à solicitação; e, após a aprovação do orçamento, o cliente agenda o serviço e acompanha o atendimento e o histórico. A jornada abaixo mostra as etapas do cliente, do problema à fidelização.
 
![Preview][jrnd]
 
**Detalhamento da jornada do usuário**
 
| | Problema | Escolha | Cadastro | Entrega | Fidelização |
|---|----------|---------|----------|---------|-------------|
| **Ações** | Percebe que tem um problema em casa. | Encontra o app, navega e filtra por área e avaliação. Compara preços e portfólios. Seleciona profissional. | Cria conta no app. Descreve o serviço (texto + fotos do problema). Escolhe data e hora. Aguarda confirmação. | Recebe o profissional. Acompanha execução. Aprova e realiza o pagamento. | Avalia o profissional. Indica para amigos. Recontratação. |
| **Pontos de Contato** | Google, rede, indicação, etc. | Tela de busca, perfil profissional, preço estimado, portfólio. | Formulário, upload de fotos, calendário, confirmação, chat. | Pagamento, comprovante, notificações, chat. | Tela de avaliação, favoritar profissional. |
| **Emoções** | Insatisfeito | Curioso | Apreensivo | Aliviado | Satisfeito |
| **Pontos de Dor** | Não sabe como achar um profissional confiável. Medo de ser enganado ou cobrado caro. | Poucos profissionais disponíveis na região. | Cadastro longo abandona o app. Demora no aceite gera ansiedade. | Profissional atrasa sem comunicação. Qualidade diferente da prometida. | Sem incentivo para voltar. Esquece o app após o serviço. |
| **Soluções** | Profissionais verificados. Avaliações públicas. | Preço estimado visível. Portfólio. | Login com Google. Timer. Status em tempo real. | Garantia de serviço. Reembolso se falhar. | Programa de pontos. Recontratar com 1 clique. |
 
### 2.6 Camada de Dados do Aplicativo
 
Seguindo o princípio de que o backend é a única fonte de verdade, o aplicativo mobile não possui um banco de dados próprio com regras de negócio. Toda informação oficial (usuários, profissionais, serviços, contratos, agendamentos, avaliações, notificações e mensagens) continua armazenada no MySQL e é acessada exclusivamente por meio da API REST. No dispositivo, os dados são mantidos apenas de forma temporária ou auxiliar, com o objetivo de agilizar a navegação, reduzir requisições repetidas e manter a sessão do usuário. Esses dados locais são sempre uma cópia de leitura e podem ser descartados e recarregados a partir da API a qualquer momento.
 
#### 2.6.1 Onde cada tipo de dado será armazenado
 
A tabela a seguir resume a estratégia de armazenamento adotada para cada categoria de dado do aplicativo.
 
| Camada | Dados armazenados | Justificativa |
|--------|-------------------|---------------|
| API (MySQL via Prisma) | Usuários, perfis de profissionais, categorias, serviços, contratos, agendamentos, avaliações, notificações e mensagens do chat. | É a fonte única de verdade, compartilhada com a versão web. Toda criação, alteração ou exclusão de dados passa pela API, sem duplicar regras de negócio no app. |
| Cloud (Cloudinary) | Imagens de perfil e imagens relacionadas aos serviços e ao portfólio do profissional. | O upload é feito por meio da API e o app guarda apenas a URL da imagem recebida, sem armazenar o arquivo no dispositivo. |
| Shared Preferences | Token JWT, identificador e tipo do usuário (cliente ou profissional), nome para exibição e indicador de que a tela "Como funciona" já foi visualizada. | São poucos dados simples no formato chave-valor, necessários para manter o usuário autenticado entre aberturas do app. O token deve ficar em armazenamento seguro do dispositivo (Keystore/Keychain), quando oferecido pelo framework escolhido. |
| Secure Storage (Keystore/Keychain) | Token JWT da sessão do usuário, quando o framework escolhido oferecer armazenamento seguro. | Mantém a credencial de autenticação em área protegida do dispositivo, em vez de em armazenamento chave-valor comum. |
| Local Storage (banco local) | Cópia temporária de categorias, profissionais, serviços, contratos, agendamentos, notificações e mensagens já consultados, além dos dados básicos do usuário logado. | Permite abrir telas já visitadas com rapidez e exibir o último conteúdo carregado quando a conexão estiver lenta ou indisponível. Não substitui a API e não é fonte de verdade. |
 
### 2.7 Tratamento de dados e implicações da LGPD para o aplicativo
 
O Contrataqui trata dados pessoais necessários para cadastro, autenticação, comunicação, contratação e acompanhamento dos serviços. O tratamento deve observar os princípios e direitos previstos na Lei nº 13.709/2018 (LGPD).
 
| Dado | Finalidade | Tratamento/cuidados |
|------|------------|---------------------|
| Nome | Identificação do usuário | Coletar apenas quando necessário e permitir atualização. |
| E-mail | Cadastro, autenticação e comunicação | Restringir acesso e utilizar para as finalidades informadas. |
| Telefone | Contato relacionado aos serviços | Limitar o acesso aos usuários que necessitem do dado. |
| Endereço | Execução/agendamento, quando necessário | Evitar exposição desnecessária e limitar o acesso. |
| Senha | Autenticação | Armazenar de forma segura, nunca em texto puro. |
| Dados do perfil profissional | Apresentação dos serviços e contratação | Disponibilizar somente o necessário ao funcionamento. |
| Histórico/avaliações | Acompanhamento dos serviços e reputação | Manter integridade, finalidade e acesso compatível. |
 
## 3. Projeto Ágil para Desenvolvimento do Aplicativo
 
Esta seção contém a documentação do projeto realizado utilizando ferramentas e métodos ágeis.
 
### 3.1 Escopo
 
**Incluído no escopo deste semestre (MVP mobile)**
 
- Cadastro e login de usuários (clientes e profissionais);
- Visualização e busca de profissionais e seus serviços;
- Visualização de perfil do profissional;
- Solicitação de orçamento pelo cliente;
- Gerenciamento de solicitações de orçamento pelo profissional (visualizar/responder);
- Agendamento de serviço após aprovação do orçamento;
- Histórico de serviços (cliente e profissional);
- Edição básica de perfil.
**Fora do escopo deste semestre (ficam para uma eventual continuidade futura do projeto)**
 
- Chat em tempo real dentro do app mobile (mantido apenas na versão web por ora, dependendo da complexidade de integração do Socket.io no framework escolhido);
- Notificações push;
- Dashboards estatísticos (existentes hoje apenas no painel administrativo web);
- Funcionalidades administrativas.
Ou seja: nem tudo que está na visão do produto será desenvolvido neste semestre, o foco é um MVP funcional cobrindo a jornada essencial de cliente e profissional.
 
### 3.2 Papéis e responsabilidades
 
| Integrante | Papel(is) | Principais responsabilidades |
|------------|-----------|------------------------------|
| Nicoly Whitehead Maria Bonani | Product Owner (PO), UX/Documentação, QA e Porta Voz | Levantamento e priorização do backlog, organização/atualização da documentação, elaboração ou validação dos protótipos de tela, testes manuais de usabilidade e reporte de bugs. |
| Leonardo Balbino | Desenvolvedor (Dev), Scrum Master | Implementação do app mobile (telas, integração com API, testes técnicos), definição de arquitetura e controle de versão no Git. |
 
### 3.3 Épicos do projeto
 
| Épico | Descrição curta | Data de entrega |
|-------|-----------------|-----------------|
| EP2 \| Fundação do app | Configuração do projeto mobile, autenticação (login/cadastro), navegação principal e integração inicial com a API existente. | Setembro de 2026 |
| EP3 \| Fluxo de negócio | Busca e visualização de profissionais, solicitação e gestão de orçamentos, agendamento de serviços. | Setembro a outubro de 2026 |
| EF \| Consolidação e entrega final | Histórico de serviços, edição de perfil, ajustes de usabilidade, testes finais e documentação final. | Outubro a novembro de 2026 |
 
### 3.4 Backlog do Produto (app) e Planejamento de Sprints
 
#### Histórias de usuário
 
Histórias de usuário priorizadas para o app mobile (adaptadas das histórias já validadas na versão web, com foco em uso móvel):
 
- Como usuário não cadastrado, quero visualizar profissionais e seus serviços no app para entender o que a plataforma oferece antes de me cadastrar.
- Como usuário não cadastrado, quero me cadastrar pelo app informando meus dados para começar a usar a plataforma pelo celular.
- Como usuário cadastrado, quero fazer login no app para acessar minha conta.
- Como usuário cadastrado, quero editar meu perfil pelo app para manter meus dados atualizados sem precisar de um computador.
- Como cliente, quero buscar profissionais pelo app para encontrar prestadores de serviço onde quer que eu esteja.
- Como cliente, quero solicitar um orçamento por meio do app para não depender de estar no site.
- Como cliente, quero agendar um serviço após a aprovação do orçamento diretamente pelo app.
- Como cliente, quero visualizar o histórico de serviços no app para acompanhar meus atendimentos anteriores.
- Como profissional, quero gerenciar meu perfil profissional pelo app para atualizar minhas informações em qualquer lugar.
- Como profissional, quero receber e responder solicitações de orçamento pelo app para não perder oportunidades enquanto estou em campo.
- Como profissional, quero gerenciar meus agendamentos pelo app para organizar minha agenda de serviços.
- Como profissional, quero visualizar meu histórico de serviços realizados pelo app.
#### Situação do backlog do produto
 
Situação do backlog do produto em 06/11/2026. A tabela abaixo apresenta cada funcionalidade com seu status atualizado (Concluído, Em execução ou Planejado).
 
| Funcionalidade | Status |
|----------------|--------|
| Cadastro de clientes e profissionais | Concluído |
| Login e autenticação | Concluído |
| Logout | Concluído |
| Esqueci minha senha | Concluído |
| Como funciona | Concluído |
| Home com as categorias de serviços | Concluído |
| Categorias | Concluído |
| Busca e visualização de profissionais/serviços | Concluído |
| Perfil profissional | Concluído |
| Notificações (tela já integrada à API; aguardando registros no banco) | Concluído |
| Solicitação de orçamento pelo cliente | Planejado |
| Resposta/gerenciamento de orçamento pelo profissional | Concluído |
| Aprovação/contratação do serviço | Concluído |
| Contrato (tela com as informações do contrato) | Concluído |
| Agendamento do serviço |  Concluído |
| Agendamentos (painel com os contratos do usuário) | Concluído |
| Histórico de serviços | Concluído |
| Edição básica de perfil | Concluído |
| Minha Conta (configurações) | Concluído |
| Avaliação do serviço/profissional | Planejado |
| Chat em tempo real no mobile | Concluído |
| Notificações push | Concluído |
| Funcionalidades administrativas | Fora do escopo |
 
#### Planejamento das sprints
 
| Mês/Épico | Sprint Nº | Objetivo da sprint | Entregáveis/Histórias |
|-----------|-----------|--------------------|-----------------------|
| Setembro – EP2 | 1 | Definir o framework mobile, configurar o projeto e criar a navegação principal. | Projeto inicial do app e estrutura das telas. |
| Setembro – EP2 | 2 | Implementar o cadastro e o login integrados à API existente. | Cadastro de clientes e profissionais; login com autenticação JWT. |
| Setembro – EP3 | 3 | Implementar a busca e a visualização dos profissionais. | Busca de profissionais, serviços e perfil profissional. |
| Outubro – EP3 | 4 | Implementar as solicitações e respostas de orçamento. | Cliente solicita orçamento; profissional visualiza e responde. |
| Outubro – EP3/EF | 5 | Implementar o agendamento de serviços. | Cliente agenda o serviço após a aprovação do orçamento. |
| Outubro – EF | 6 | Implementar o histórico e a edição de perfil. | Histórico de serviços e edição básica do perfil. |
| Novembro – EF | 7 | Realizar testes e corrigir problemas. | Testes de usabilidade, testes técnicos e correção de bugs. |
| Novembro – EF | 8 | Finalizar a documentação e preparar a entrega. | Documentação atualizada, versão final e apresentação. |
 
### 3.5 Definição de preparada (Ready)
 
Uma história está preparada quando possui descrição clara, critérios de aceitação definidos, prioridade estabelecida, dependências identificadas e todas as informações necessárias para iniciar o desenvolvimento.
 
### 3.6 Definição de pronta (Done)
 
Uma história será considerada pronta quando estiver implementada, integrada à API quando necessário e testada em um dispositivo Android ou emulador. Ela também deverá atender aos critérios de aceitação, não quebrar funcionalidades existentes, estar de acordo com o protótipo do Figma e ter seu código versionado no GitHub.
 
### 3.7 Gestão de riscos
 
| Risco | Probabilidade | Impacto | Estratégia de Mitigação |
|-------|---------------|---------|-------------------------|
| Demora na definição entre React e Flutter. | Média | Alto | Definir o framework na primeira sprint e evitar trabalhar com as duas opções ao mesmo tempo. |
| Dificuldade de aprendizado do framework escolhido. | Média | Média | Reservar a primeira sprint para estudos, configuração e criação de um protótipo simples. |
| A API não possuir algum endpoint necessário. | Média | Alto | Verificar os endpoints antes de cada sprint e solicitar alterações no backend quando necessário. |
| Pouco tempo disponível fora das aulas. | Alta | Médio | Definir objetivos pequenos e realizar entregas parciais em cada sprint. |
| Problemas de integração entre o app e a API. | Média | Médio | Reaproveitar os endpoints, o JWT e os formatos de dados já utilizados pela versão web. |
| Problemas na integração com o Cloudinary. | Média | Médio | Testar o upload e a exibição das imagens separadamente. |
 
## 4. Conclusão
 
Este documento apresentou o Plano de Projeto Ágil de Software do Contrataqui Mobile, a versão para smartphones da plataforma Contrataqui, que conecta clientes a profissionais prestadores de serviço. Foram descritos o contexto do aplicativo e do negócio, a visão do produto, a arquitetura, a camada de dados, o tratamento de dados conforme a LGPD e a organização do trabalho em métodos ágeis, com papéis, épicos, backlog, sprints, definições de preparada e pronta e gestão de riscos.
 
O aplicativo Contrataqui cumpre os objetivos propostos no projeto ágil de desenvolvimento do app: consome a mesma API REST da versão web, mantém o backend como única fonte de verdade e cobre, em um MVP, a jornada essencial de clientes e profissionais. Dessa forma, também cumpre os objetivos do Projeto Integrador do 5º semestre do curso superior de Desenvolvimento de Software Multiplataforma.
 
Como limitação, conforme descrito no escopo, o chat em tempo real dentro do app, as notificações push, os dashboards estatísticos e as funcionalidades administrativas não foram desenvolvidos neste semestre. Essas funcionalidades serão desenvolvidas em trabalhos futuros.

[arqu]: imgs_cntrtqi/arquitetura.jpeg
[jrnd]: imgs_cntrtqi/jornada.jpeg
 
