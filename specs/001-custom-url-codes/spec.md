# Feature Specification: Códigos Customizados para URLs

**Feature Branch**: Not created

**Created**: 2026-10-29

**Status**: Draft

**Input**: User description: "Permitir que o usuário informe um código customizado ao encurtar uma URL, para depois navegar por ele (ex.: /balta-io em vez de /aZ3k9Qx). A criação atual recebe apenas a URL e gera o código automaticamente. O código customizado deve ser opcional; quando válido, deve ser único, obedecer a regras de formato e não conflitar com rotas da API. Listagem, detalhes, redirecionamento e contagem de cliques devem continuar funcionando. Fora do escopo: editar ou remover o código depois de criado, autenticação e expiração de links."

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Create and use a custom code (Priority: P1)

A pessoa informa uma URL e um código customizado válido ao criar um link curto. O endereço curto utiliza exatamente o código escolhido e, ao ser acessado, encaminha a pessoa para a URL original.

**Why this priority**: Permite que as pessoas escolham endereços curtos reconhecíveis, que é o principal valor da funcionalidade.

**Independent Test**: Criar um link com o código `balta-io`, conferir o endereço curto retornado e acessá-lo para confirmar o destino e a contagem de cliques.

**Acceptance Scenarios**:

1. **Given** uma URL válida e um código customizado válido e ainda não utilizado, **When** a pessoa cria o link curto, **Then** o link usa exatamente o código informado.
2. **Given** um link criado com código customizado, **When** uma pessoa acessa o endereço curto, **Then** é encaminhada à URL original e a contagem de cliques aumenta uma vez.

### User Story 2 - Receive clear feedback for unavailable codes (Priority: P1)

A pessoa recebe uma explicação quando tenta criar um link com código duplicado, reservado ou fora do formato permitido, sem que um link seja criado.

**Why this priority**: Evita endereços ambíguos ou incompatíveis com as rotas do serviço e torna as regras previsíveis para quem cria links.

**Independent Test**: Tentar criar links com um código já existente, com `urls`, com caracteres não permitidos e com comprimentos fora dos limites; verificar a rejeição, a mensagem correspondente e a ausência de novos links.

**Acceptance Scenarios**:

1. **Given** um código já atribuído a outro link, **When** a pessoa tenta utilizá-lo, **Then** a criação é recusada com a mensagem de que o código está em uso.
2. **Given** um código reservado ou fora do formato permitido, **When** a pessoa tenta utilizá-lo, **Then** a criação é recusada com uma mensagem que identifica o motivo.
3. **Given** uma tentativa recusada, **When** a pessoa consulta os links existentes, **Then** nenhum link novo foi criado por essa tentativa.

### User Story 3 - Continue using generated codes and existing views (Priority: P2)

A pessoa continua podendo criar links sem escolher um código e consultar links com código gerado ou customizado da mesma forma.

**Why this priority**: Preserva o fluxo atual e evita que a opção customizada quebre o uso existente.

**Independent Test**: Criar um link sem informar código e conferir que ele recebe um código gerado; depois confirmar que links gerados e customizados aparecem na listagem e podem ser consultados individualmente.

**Acceptance Scenarios**:

1. **Given** uma URL válida e nenhum código informado, **When** a pessoa cria o link curto, **Then** o serviço gera um código automaticamente como antes.
2. **Given** links com códigos gerados e customizados, **When** a pessoa lista ou consulta esses links, **Then** os dados são apresentados no mesmo formato e incluem o código correto.

### Edge Cases

- Códigos com 2 caracteres ou 33 caracteres são recusados; os limites de 3 e 32 caracteres são aceitos quando os demais critérios são atendidos.
- Letras, números, hífen e underscore são aceitos; espaços, pontuação e caracteres não ASCII são recusados.
- O código `urls` é reservado sem distinção entre maiúsculas e minúsculas, pois conflita com uma rota existente do serviço.
- Códigos customizados são comparados e preservados respeitando maiúsculas e minúsculas; uma repetição exata é recusada.
- Campo de código ausente, nulo, vazio ou composto apenas por espaços é tratado como não informado; espaços dentro de um código não vazio tornam o código inválido.
- A navegação para um código inexistente continua sem redirecionamento e sem incrementar cliques.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: O fluxo existente de criação MUST aceitar um código customizado opcional. Quando o código não for informado, o serviço MUST continuar gerando um código automaticamente.
- **FR-002**: O serviço MUST aceitar códigos customizados com comprimento entre 3 e 32 caracteres, inclusive, compostos somente por letras ASCII, números, hífen ou underscore.
- **FR-003**: O serviço MUST preservar o código customizado válido exatamente como informado e utilizá-lo no endereço curto.
- **FR-004**: O serviço MUST recusar a criação quando o código já estiver atribuído a outro link, informar que o código está em uso e não criar um link nessa tentativa.
- **FR-005**: O serviço MUST recusar códigos que conflitem com rotas existentes. O código `urls` MUST ser reservado sem distinção entre maiúsculas e minúsculas.
- **FR-006**: O serviço MUST informar se um código foi recusado por formato inválido ou por ser reservado, sem criar um link.
- **FR-007**: A navegação por um link curto com código customizado MUST encaminhar para a URL original e incrementar a contagem de cliques uma vez por acesso bem-sucedido, como ocorre para códigos gerados.
- **FR-008**: A listagem e a consulta individual MUST apresentar links com código customizado da mesma forma que links com código gerado.
- **FR-009**: O serviço MUST manter os links já criados e o fluxo de criação sem código customizado compatíveis com o comportamento atual.

### Key Entities _(include if feature involves data)_

- **Link curto**: Associação entre um código único, a URL original, o endereço curto, a data de criação e a contagem de cliques. O código pode ser gerado pelo serviço ou escolhido pela pessoa na criação.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: Em todos os testes de criação com código customizado válido e disponível, o endereço curto apresentado contém exatamente o código escolhido.
- **SC-002**: 100% das tentativas com código duplicado, reservado ou fora do formato são recusadas com o motivo informado e sem criar um link.
- **SC-003**: Em todos os testes de navegação bem-sucedida, links com códigos gerados e customizados levam à URL original e registram exatamente um clique por acesso.
- **SC-004**: Em todos os testes de criação sem código, listagem e consulta individual, o comportamento permanece compatível com o fluxo atual.
- **SC-005**: As mensagens de recusa permitem que a pessoa identifique claramente se o código já está em uso, é reservado ou tem formato inválido.

## Assumptions

- Como os limites não foram especificados, o tamanho mínimo será 3 e o máximo será 32 caracteres.
- "Letras" significa letras ASCII de A a Z, sem acentos; códigos customizados diferenciam maiúsculas de minúsculas e preservam a capitalização informada.
- Campo ausente, nulo ou em branco equivale a não informar um código; se houver qualquer caractere não branco, o valor inteiro deve atender às regras de formato, sem remoção ou normalização de espaços.
- `urls` é o nome de rota conflitante conhecido no serviço atual; nomes de novas rotas também deverão ser reservados caso passem a conflitar com códigos de um único segmento.
- A edição ou remoção do código após a criação, autenticação e expiração de links permanecem fora do escopo.
