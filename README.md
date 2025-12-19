# Integrador MEK7300 – Visão Geral e Guia de Uso

Este projeto é um **integrador para o equipamento de hemograma MEK7300**. Seu objetivo é **ler os dados do exame via comunicação serial**, transformar essas informações em um **arquivo de texto padronizado** e **enviar os dados para um webhook** configurado.

O funcionamento foi pensado para ser simples, rastreável via logs e fácil de manter, mesmo por pessoas que não tiveram contato prévio com o projeto.

---

## Visão Geral do Fluxo

O fluxo completo do integrador funciona da seguinte forma:

1. O serviço escuta continuamente a **porta serial** do equipamento MEK7300.
2. Ao receber um exame completo, os dados brutos são lidos e validados.
3. Os dados são processados e transformados em um **arquivo `.txt` padronizado**.
4. O arquivo é salvo na pasta **`Gerados`**.
5. A cada 1 minuto, o serviço verifica a pasta `Gerados`.
6. Cada arquivo é enviado para o **webhook configurado**.
7. Se o webhook responder **HTTP 200 (OK)**:

   * O arquivo é movido para a pasta **`Processados`**.
8. Se ocorrer erro no envio:

   * O arquivo permanece na pasta `Gerados` para nova tentativa.
9. Todo o processo é registrado em **logs**.

---

## Estrutura de Pastas Importantes

* **Gerados/**

   * Contém os arquivos de exames que ainda **não foram enviados** ou que **falharam no envio**.

* **Processados/**

   * Contém os arquivos que já foram enviados com sucesso ao webhook.

* **docs/**

   * Documentação técnica e arquivos de exemplo (mocks) para desenvolvedores.

---

## Instalação e Configuração

### 1. Compilar o Projeto

Certifique-se de compilar o projeto e gerar o executável:

* `MEK7300service.exe`

---

### 2. Executar o Prompt como Administrador

No Windows:

* Clique com o botão direito no **Prompt de Comando**
* Selecione **Executar como Administrador**

---

### 3. Criar o Serviço no Windows

Execute o comando abaixo, ajustando o caminho do executável conforme necessário:

```
sc create mek7300listener binPath= "C:\CAMINHO\PARA\MEK7300service.exe"
```

---

### 4. Verificação Inicial

Após iniciar o serviço, verifique se as seguintes pastas foram criadas automaticamente:

* `Gerados`
* `Processados`

Essas pastas são essenciais para o funcionamento do integrador.

---

## Configurações via appsettings.json

O arquivo `appsettings.json` contém as principais configurações do serviço.

### Webhook

* Chave: `WebHook`
* Define a URL para onde os exames serão enviados.

### Logs

* Chave: `LogFilePath`
* Define o caminho e nome do arquivo de log.
* Por padrão, o log se chama:

   * `service.log`

---

## Estrutura do Código e Fluxo Interno

### Leitura da Porta Serial

O ponto inicial do sistema é o arquivo:

* **`SerialListener.cs`**

Responsabilidades:

* Escutar a comunicação serial do equipamento MEK7300
* Identificar quando um exame foi completamente recebido
* Encaminhar os dados brutos para processamento

---

### Processamento e Geração do Arquivo

Após a leitura completa do exame, o fluxo segue para:

* **`ProccessFile.cs`**

Responsabilidades:

* Mapear os dados recebidos da máquina
* Validar o conteúdo
* Montar o arquivo final no formato esperado
* Salvar o arquivo na pasta **`Gerados`**

O formato final do arquivo segue o modelo disponível em:

* [mock_arquivo_final_para_envio.txt](docs/mock_arquivo_final_para_envio.txt)

---

## Comunicação Serial do Equipamento

A comunicação do MEK7300 ocorre via **porta serial**, seguindo um formato específico.

A documentação completa da pinagem e do protocolo está disponível em:

* `docs/Pinagem serial MEKs.pdf`

---

## Arquivos de Mock (Importante para Desenvolvedores)

Dentro da pasta `docs/` existem arquivos de exemplo que ajudam a entender e manter o projeto.

### 1. mock_buffer_completo.txt

Arquivo:

* [mock_buffer_completo.txt](docs/mock_buffer_completo.txt)

Descrição:

* Representa **como os dados devem chegar completos** para a classe de processamento.
* Contém caracteres especiais que indicam início e fim do conteúdo.

#### Caracteres de Controle

* **STX (Start of Text)**

   * ASCII: 2
   * Hex: `0x02`
   * Unicode: `U+0002`

* **ETX (End of Text)**

   * ASCII: 3
   * Hex: `0x03`
   * Unicode: `U+0003`

Esses caracteres são obrigatórios para identificar corretamente o bloco de dados.

---

### 2. mock_arquivo_final_para_envio.txt

Arquivo:

* [mock_arquivo_final_para_envio.txt](docs/mock_arquivo_final_para_envio.txt)

Descrição:

* Representa exatamente **como o arquivo final deve estar antes do envio ao webhook**.
* Serve como referência para validação do processamento.

---

## Nome do Arquivo de Exame

* O arquivo gerado é do tipo `.txt`
* O **nome do arquivo deve conter o Sample ID** (número da amostra)
* Esse número é digitado diretamente no equipamento de hemograma

Esse padrão é essencial para rastreabilidade dos exames.

---

## Ciclo Completo do Integrador

Resumidamente, o integrador:

* Lê dados da porta serial
* Processa e gera um arquivo `.txt`
* Armazena temporariamente em `Gerados`
* Envia para um webhook
* Move para `Processados` em caso de sucesso
* Registra todo o processo em logs

---

## Logs e Monitoramento

Todo o funcionamento do serviço pode ser acompanhado via arquivo de log.

* Nome padrão: `service.log`
* Configurável em: `appsettings.json`

Os logs são fundamentais para:

* Diagnóstico de falhas
* Validação de envios
* Acompanhamento de exames processados

---

## Documentação Adicional

Para um passo a passo detalhado de instalação e contexto do integrador, consulte a Wiki:

* [Manual de instalação do integrador Vida Exames com MEK7300](https://wiki.suporte.vidaexame.com/pt-br/Manual-de-instala%C3%A7%C3%A3o-integrador-Vida-Exames-com-MEK7300)
