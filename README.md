# WeatherApp

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![WPF](https://img.shields.io/badge/WPF-Windows_Desktop-0078D4?style=flat&logo=windows)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Aplicação WPF simples que consulta a API OpenWeatherMap e exibe informações básicas do tempo (cidade, horário local, descrição, temperatura, sensação térmica, umidade e velocidade do vento). Projeto de exemplo voltado para aprendizado de integração HTTP, desserialização JSON e construção de UI com XAML.

![Demonstração da aplicação](WeatherApp/Resources/GitHubResources/20260130_161005_Export.GIF)

---

## Sumário

- Visão geral
- Estrutura do projeto
- Componentes principais
  - UI (`MainWindow.xaml`)
  - Lógica (`MainWindow.xaml.cs`)
  - Modelos (`Models/*.cs`)
- Como a conexão com a API foi feita
- Fluxo de dados (request → desserialização → UI)
- Como configurar e rodar
- Possíveis problemas conhecidos e correções recomendadas
- Melhorias sugeridas
- Contribuição
- Licença

---

## Visão geral

`WeatherApp` é uma aplicação desktop WPF que faz uma requisição HTTP para o endpoint `https://api.openweathermap.org/data/2.5/weather` e exibe os resultados na interface. O foco é demonstrar a integração com uma API REST, manipulação do JSON retornado e atualização de controles XAML.

---

## Estrutura do projeto

Principais arquivos e pastas:

- `WeatherApp/`
  - `View/`
    - `MainWindow.xaml` — definição da interface (XAML).
    - `MainWindow.xaml.cs` — code-behind com comportamento e chamadas HTTP.
  - `Models/`
    - `WeatherResponse.cs` — modelo principal que espelha a resposta da API.
    - `MainInfo.cs` — mapeia `main` (temperatura, feels_like, humidity).
    - `WeatherInfo.cs` — mapeia `weather` (description).
    - `WindInfo.cs` — mapeia `wind` (speed).
  - `Resources/` — imagens usadas como background/ilustração conforme descrição do tempo.
  - `App.xaml` — inicialização WPF.
  
---

## Componentes principais

### UI — `MainWindow.xaml`

Estilos reutilizáveis:
- `WheaterInfoLabel` e `WheaterInfoTextBlock` para padronizar aparência de labels e valores.

Controles nomeados usados na lógica:
- `SearchBox`, `SearchTextBlock`, `btnSearch`
- `CityResult`, `TimezoneResult`, `DescriptionResult`
- `TempResult`, `FeelsLikeResult`, `HumidityResult`, `WindResult`
- `WeatherImage`

### Lógica — `MainWindow.xaml.cs`

- Usa `HttpClient` (`private readonly HttpClient _httpClient = new HttpClient();`) para requisições.
- API key hard-coded em `apiKey`.
- `Button_Click` (evento do botão `OK`):
  - Lê `SearchBox.Text`.
  - Constroi URL com `units=metric` e `lang=en_eua`.
  - Faz `GetAsync(url)` e lê o conteúdo como string.
  - Desserializa com `System.Text.Json` e `PropertyNameCaseInsensitive = true` para mapear propriedades do JSON sem sensibilidade a maiúsculas/minúsculas.
  - Atualiza controles de UI com valores do objeto desserializado:
    - `CityResult.Text = weather.Name`
    - `TimezoneResult.Text = localTime` (converte `weather.Timezone` em horas/minutos)
    - `TempResult.Text`, `FeelsLikeResult.Text`, `HumidityResult.Text`, `WindResult.Text`
  - Decide qual imagem usar em `WeatherImage` com base em `weather.Weather[0].Description` e se é dia/noite (calculado a partir do horário local).
  - `SearchBox_TextChanged` gerencia o placeholder (`SearchTextBlock.Visibility`).
  - Handlers para minimizar/fechar e arrastar a janela.

### Modelos — `Models/*.cs`

- `WeatherResponse`:
  - `string? Name`
  - `int Timezone`
  - `MainInfo? Main`
  - `required WeatherInfo[] Weather`
  - `WindInfo? Wind`
- `MainInfo`:
  - `double Temp`
  - `[JsonPropertyName("feels_like")] double FeelsLike`
  - `double Humidity`
- `WeatherInfo`:
  - `string Description`
- `WindInfo`:
  - `double Speed`

Esses modelos são usados pelo `JsonSerializer` para mapear o JSON da API.

---

## Como a conexão com a API foi feita

1. Montagem da URL:
   - `https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=pt_br`
   - `units=metric` para Celsius.
   - `lang=pt_br` para descrições em português.

2. Requisição:
   - `var httpResponse = await _httpClient.GetAsync(url);`
   - Verifica status (no código atual a verificação ocorre após a leitura/desserialização — ver observações abaixo).

3. Desserialização:
   - `var weather = JsonSerializer.Deserialize<WeatherResponse>(response, options);`
   - `options.PropertyNameCaseInsensitive = true` para flexibilidade no mapeamento.

4. Atualização da UI com os dados desserializados.

---

## Fluxo de dados (request → desserialização → UI)

1. Usuário insere cidade em `SearchBox` e clica `OK`.
2. `Button_Click` chama a API com `HttpClient`.
3. Conteúdo JSON é desserializado para `WeatherResponse`.
4. O code-behind lê propriedades (temperatura, feels_like, humidity, wind.speed, weather[0].description, timezone).
5. Atualiza `TextBlock`s e `Image` com os valores correspondentes.

---

## Possíveis problemas conhecidos (e diagnóstico observado)

Observação importante encontrada durante análise do código:

- O método `Button_Click` acessa `weather.Main.Temp`, `weather.Weather[0].Description`, etc. imediatamente após a desserialização, mas **antes** de checar se `httpResponse.IsSuccessStatusCode` e antes de validar se `weather` / `weather.Main` / `weather.Weather` não são nulos. Se a resposta for mal formada, nula ou incompleta, o acesso pode lançar `NullReferenceException`. Esse `catch` geral do `try/catch` então limpa os `TextBlock`s (define string vazia), fazendo com que nada seja visível para temperatura, sensação térmica e umidade — exatamente o sintoma reportado.

- O comportamento observado ("Wind retorna porque eu inseri manualmente") é consistente: você pode ter inserido valores manualmente para `WindResult` durante testes; mas quando a execução normal entra em exceção o `catch` limpa os campos. Ou o `Wind` pode ter sido desserializado com sucesso enquanto `Main` era nulo (dependendo do JSON), permitindo que o vento fosse mostrado mas os campos de `main` ficassem vazios.

- Outras fragilidades:
  - Validação de `SearchBox` ocorre depois de chamar a API.
  - Cálculo de hora local depende de `TimezoneResult.Text.Substring(0,2)`, o que é frágil caso `TimezoneResult` esteja vazio ou em formato inesperado.
  - `apiKey` está hard-coded no código-fonte (não seguro para produção).

Recomendação imediata: reordenar validações — verificar campo de cidade antes de chamar a API; verificar `IsSuccessStatusCode` e validar `weather`/`weather.Main`/`weather.Weather` antes de acessar suas propriedades.

Exemplo de correção (resumo):
- Se `string.IsNullOrWhiteSpace(city)` → mostrar mensagem e `return`.
- Após `GetAsync`, se `!httpResponse.IsSuccessStatusCode` → atualizar UI com mensagem específica e `return`.
- Após desserializar, se `weather == null || weather.Main == null || weather.Weather == null || weather.Weather.Length == 0` → atualizar UI com mensagem e `return`.
- Só então acessar `weather.Main.Temp` e atualizar `TempResult`, `FeelsLikeResult`, `HumidityResult`.

---

## Como configurar e rodar (local)

Pré-requisitos:
- .NET 10 SDK
- Visual Studio (recomendado) com workload de desenvolvimento WPF instalado

Passos:
1. Clone o repositório:
   - `git clone https://github.com/fernandoGonzaga0/WeatherApp.git`
2. Abra a solução/projeto no Visual Studio.
3. Defina sua API key do OpenWeatherMap:
   - Atualmente a chave está em `WeatherApp/View/MainWindow.xaml.cs` na variável `apiKey`. Para maior segurança, substitua esse uso por leitura de uma variável de ambiente ou arquivo de configurações:
     - Exemplo (variável de ambiente): `Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY")`
   - Crie uma conta em https://openweathermap.org/ e gere uma API key.
4. Build e Execute (`F5`).
5. Na UI, digite o nome da cidade e clique em `OK`.

---

## Depuração recomendada

- Coloque breakpoint logo após `JsonSerializer.Deserialize(...)` e inspecione `weather`.
- Use a janela __Output__ > __Debug__ para ver logs do `Debug.WriteLine`.
- Verifique o valor do `httpResponse.StatusCode` se a API retornar 4xx/5xx.

---

## Melhorias sugeridas

- Adotar MVVM (separar UI de lógica) e usar `INotifyPropertyChanged` para binding direto ao ViewModel em vez de manipular controles no code-behind.
- Remover a API key do código-fonte; usar `user secrets`, `appsettings.json`, ou variáveis de ambiente.
- Usar `IHttpClientFactory` (quando aplicável) para gerenciamento de `HttpClient`.
- Tratar erros de rede com feedback ao usuário e retry/backoff se necessário.
- Internacionalização completa (usando `Resources`), caso deseje suportar múltiplos idiomas.
- Validar e proteger contra `NullReferenceException` com checagens e uso de `?.` e `??`.
- Melhorar cálculo dia/noite usando `weather.Timezone` e `DateTimeOffset` em vez de string-parsing.

---

## Contribuição

1. Fork e clone o projeto.
2. Crie branch com feature/bugfix.
3. Abra PR com descrição clara das mudanças.
4. Mantenha commits pequenos e mensagens descritivas.

---

## Licença

Adicionar arquivo `LICENSE` conforme preferência (ex.: MIT) ou especificar a licença no repo.

---

Se quiser, eu posso:
- Gerar um patch que reordene as validações e corrija o bug que limpa os campos ao lançar exceção.
- Remover a chave hard-coded e implementar leitura de variável de ambiente.
- Extrair a lógica para um `ViewModel` (adotar MVVM).
