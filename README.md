## Instalação e Configuração

1. **Compile o Projeto**
   - Certifique-se de compilar o projeto e gerar o arquivo executável (`MEK7300service.exe`).

2. **Abra o Prompt de Comando como Administrador**
   - Clique com o botão direito no ícone do Prompt de Comando e selecione **Executar como Administrador**.

3. **Crie o Serviço**
   Execute o comando abaixo para criar o serviço no Windows:
   ```cmd
   sc create mek7300listener binPath= "C:\CAMINHO\PARA\MEK7300service.exe"
   ```

#### Verifique a criação das pastas 'gerados' e 'processados'


Ajuda para desenvolvedores que forem mexer no projeto
olhe a pasta [docs](docs), dentro dela tem 2 mocks(arquivos modelos)

[mock_buffer_completo.txt](docs/mock_buffer_completo.txt) -> esse arquivo é o exemplo de como deve ficar os dados
para quando forem ser enviados para a classe de processar o arquivo, 2 pontos importantens são que no começo do arquivo
existe um carater para indicar o começo um para indicaro o final 
   
STX (Start of Text)
Código ASCII: 2
Hex: 0x02
Unicode: U+0002

ETX (End of Text)
Código ASCII: 3
Hex: 0x03
Unicode: U+0003