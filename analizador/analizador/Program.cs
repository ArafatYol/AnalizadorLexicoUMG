using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace analizador
{
    public enum TipoToken
    {
        PALABRA_CLAVE,
        IDENTIFICADOR,
        NUMERO,
        CADENA,
        OPERADOR,
        DELIMITADOR,
        COMENTARIO,
        ESPACIO_BLANCO,
        DESCONOCIDO
    }

    public class Token
    {
        public TipoToken Tipo { get; set; }
        public string Valor { get; set; }
        public int Linea { get; set; }
        public int Columna { get; set; }

        public Token(TipoToken tipo, string valor, int linea, int columna)
        {
            Tipo = tipo;
            Valor = valor;
            Linea = linea;
            Columna = columna;
        }

        public override string ToString()
        {
            string tipoEnEspanol = "";

            switch (Tipo)
            {
                case TipoToken.PALABRA_CLAVE:
                    tipoEnEspanol = "PALABRA CLAVE";
                    break;
                case TipoToken.IDENTIFICADOR:
                    tipoEnEspanol = "IDENTIFICADOR";
                    break;
                case TipoToken.NUMERO:
                    tipoEnEspanol = "NÚMERO";
                    break;
                case TipoToken.CADENA:
                    tipoEnEspanol = "CADENA";
                    break;
                case TipoToken.OPERADOR:
                    tipoEnEspanol = "OPERADOR";
                    break;
                case TipoToken.DELIMITADOR:
                    tipoEnEspanol = "DELIMITADOR";
                    break;
                case TipoToken.COMENTARIO:
                    tipoEnEspanol = "COMENTARIO";
                    break;
                case TipoToken.ESPACIO_BLANCO:
                    tipoEnEspanol = "ESPACIO EN BLANCO";
                    break;
                case TipoToken.DESCONOCIDO:
                    tipoEnEspanol = "DESCONOCIDO";
                    break;
            }

            return $"'{Valor}' - {tipoEnEspanol} (Línea {Linea}, Columna {Columna})";
        }
    }

    public class Lexer
    {
        private string _entrada;
        private int _posicion;
        private int _linea;
        private int _columna;
        private List<string> _palabrasClaves;

        public Lexer(string entrada, List<string> palabrasClaves)
        {
            _entrada = entrada;
            _posicion = 0;
            _linea = 1;
            _columna = 1;
            _palabrasClaves = palabrasClaves;
        }

        private char VerSiguiente(int desplazamiento = 0)
        {
            int posicion = _posicion + desplazamiento;
            if (posicion >= _entrada.Length)
                return '\0';
            return _entrada[posicion];
        }

        private void Avanzar()
        {
            if (_posicion < _entrada.Length)
            {
                if (VerSiguiente() == '\n')
                {
                    _linea++;
                    _columna = 1;
                }
                else
                {
                    _columna++;
                }
                _posicion++;
            }
        }

        private Token AnalizarIdentificador()
        {
            int columnaInicial = _columna;
            StringBuilder sb = new StringBuilder();

            while (char.IsLetterOrDigit(VerSiguiente()) || VerSiguiente() == '_')
            {
                sb.Append(VerSiguiente());
                Avanzar();
            }

            string valor = sb.ToString();
            TipoToken tipo = _palabrasClaves.Contains(valor.ToLower()) ? TipoToken.PALABRA_CLAVE : TipoToken.IDENTIFICADOR;

            return new Token(tipo, valor, _linea, columnaInicial);
        }

        private Token AnalizarNumero()
        {
            int columnaInicial = _columna;
            StringBuilder sb = new StringBuilder();
            bool tienePuntoDecimal = false;

            while (char.IsDigit(VerSiguiente()) || (VerSiguiente() == '.' && !tienePuntoDecimal))
            {
                if (VerSiguiente() == '.')
                    tienePuntoDecimal = true;

                sb.Append(VerSiguiente());
                Avanzar();
            }

            return new Token(TipoToken.NUMERO, sb.ToString(), _linea, columnaInicial);
        }

        private Token AnalizarCadena()
        {
            int columnaInicial = _columna;
            StringBuilder sb = new StringBuilder();

            sb.Append(VerSiguiente());
            Avanzar();

            while (VerSiguiente() != '"' && VerSiguiente() != '\'' && VerSiguiente() != '\0')
            {
                if (VerSiguiente() == '\\' && (VerSiguiente(1) == '"' || VerSiguiente(1) == '\''))
                {
                    sb.Append(VerSiguiente());
                    Avanzar();
                }

                sb.Append(VerSiguiente());
                Avanzar();
            }

            if (VerSiguiente() == '"' || VerSiguiente() == '\'')
            {
                sb.Append(VerSiguiente());
                Avanzar();
                return new Token(TipoToken.CADENA, sb.ToString(), _linea, columnaInicial);
            }
            else
            {
                return new Token(TipoToken.DESCONOCIDO, sb.ToString(), _linea, columnaInicial);
            }
        }

        private Token AnalizarComentario()
        {
            int columnaInicial = _columna;
            StringBuilder sb = new StringBuilder();

            if (VerSiguiente() == '/' && VerSiguiente(1) == '/')
            {
                sb.Append(VerSiguiente());
                Avanzar();
                sb.Append(VerSiguiente());
                Avanzar();

                while (VerSiguiente() != '\n' && VerSiguiente() != '\0')
                {
                    sb.Append(VerSiguiente());
                    Avanzar();
                }
            }
            else if (VerSiguiente() == '/' && VerSiguiente(1) == '*')
            {
                sb.Append(VerSiguiente());
                Avanzar();
                sb.Append(VerSiguiente());
                Avanzar();

                while (!(VerSiguiente() == '*' && VerSiguiente(1) == '/') && VerSiguiente() != '\0')
                {
                    sb.Append(VerSiguiente());
                    Avanzar();
                }

                if (VerSiguiente() == '*' && VerSiguiente(1) == '/')
                {
                    sb.Append(VerSiguiente());
                    Avanzar();
                    sb.Append(VerSiguiente());
                    Avanzar();
                }
            }

            return new Token(TipoToken.COMENTARIO, sb.ToString(), _linea, columnaInicial);
        }

        private Token AnalizarOperador()
        {
            int columnaInicial = _columna;
            string valor = VerSiguiente().ToString();
            TipoToken tipo = TipoToken.OPERADOR;

            if ((VerSiguiente() == '+' && VerSiguiente(1) == '+') ||
                (VerSiguiente() == '-' && VerSiguiente(1) == '-') ||
                (VerSiguiente() == '=' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '!' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '<' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '>' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '&' && VerSiguiente(1) == '&') ||
                (VerSiguiente() == '|' && VerSiguiente(1) == '|') ||
                (VerSiguiente() == '+' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '-' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '*' && VerSiguiente(1) == '=') ||
                (VerSiguiente() == '/' && VerSiguiente(1) == '='))
            {
                valor = VerSiguiente().ToString() + VerSiguiente(1).ToString();
                Avanzar();
                Avanzar();
            }
            else
            {
                Avanzar();
            }

            return new Token(tipo, valor, _linea, columnaInicial);
        }

        private Token AnalizarEspacioBlanco()
        {
            int columnaInicial = _columna;
            StringBuilder sb = new StringBuilder();

            while (_posicion < _entrada.Length && char.IsWhiteSpace(VerSiguiente()))
            {
                sb.Append(VerSiguiente());
                Avanzar();
            }

            return new Token(TipoToken.ESPACIO_BLANCO, sb.ToString(), _linea, columnaInicial);
        }

        public Token ObtenerSiguienteToken()
        {
            if (_posicion >= _entrada.Length)
            {
                return null;
            }

            if (char.IsWhiteSpace(VerSiguiente()))
            {
                return AnalizarEspacioBlanco();
            }

            if (char.IsLetter(VerSiguiente()) || VerSiguiente() == '_')
            {
                return AnalizarIdentificador();
            }

            if (char.IsDigit(VerSiguiente()))
            {
                return AnalizarNumero();
            }

            if (VerSiguiente() == '"' || VerSiguiente() == '\'')
            {
                return AnalizarCadena();
            }

            if (VerSiguiente() == '/' && (VerSiguiente(1) == '/' || VerSiguiente(1) == '*'))
            {
                return AnalizarComentario();
            }

            if (VerSiguiente() == '+' || VerSiguiente() == '-' || VerSiguiente() == '*' || VerSiguiente() == '/' ||
                VerSiguiente() == '=' || VerSiguiente() == '<' || VerSiguiente() == '>' || VerSiguiente() == '!' ||
                VerSiguiente() == '&' || VerSiguiente() == '|' || VerSiguiente() == '%' || VerSiguiente() == '^')
            {
                return AnalizarOperador();
            }

            if (VerSiguiente() == ';' || VerSiguiente() == ',' || VerSiguiente() == '.' ||
                VerSiguiente() == '(' || VerSiguiente() == ')' || VerSiguiente() == '[' || VerSiguiente() == ']' ||
                VerSiguiente() == '{' || VerSiguiente() == '}' || VerSiguiente() == ':')
            {
                int columnaInicial = _columna;
                string valor = VerSiguiente().ToString();
                Avanzar();
                return new Token(TipoToken.DELIMITADOR, valor, _linea, columnaInicial);
            }

            int columnaDesconocida = _columna;
            string valorDesconocido = VerSiguiente().ToString();
            Avanzar();
            return new Token(TipoToken.DESCONOCIDO, valorDesconocido, _linea, columnaDesconocida);
        }

        public List<Token> AnalizarTokens()
        {
            List<Token> tokens = new List<Token>();
            Token token;

            while ((token = ObtenerSiguienteToken()) != null)
            {
                tokens.Add(token);
            }

            return tokens;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Analizador Léxico ===");

            List<string> palabrasClaves = new List<string>();
            bool continuarAgregando = true;

            while (palabrasClaves.Count < 3 && continuarAgregando)
            {
                Console.WriteLine($"\nToken {palabrasClaves.Count + 1}/3:");
                Console.Write("Ingrese token (o presione Enter para terminar): ");
                string token = Console.ReadLine().Trim().ToLower();

                if (string.IsNullOrEmpty(token))
                {
                    continuarAgregando = false;
                }
                else
                {
                    palabrasClaves.Add(token);
                    Console.WriteLine($"Token '{token}' agregado como palabra clave.");

                    if (palabrasClaves.Count < 3)
                    {
                        Console.Write("¿Desea agregar otro token? (s/n): ");
                        string respuesta = Console.ReadLine().Trim().ToLower();
                        continuarAgregando = (respuesta == "s" || respuesta == "si" || respuesta == "sí");
                    }
                }
            }

            Console.WriteLine("\nPalabras clave definidas:");
            foreach (string palabra in palabrasClaves)
            {
                Console.WriteLine($"- {palabra}");
            }

            Console.WriteLine("\nIngrese su cadena de texto presione enter y (escriba 'ANALIZAR' en una línea nueva para comenzar el análisis):");

            StringBuilder constructorCodigo = new StringBuilder();
            string linea;

            while ((linea = Console.ReadLine()) != "ANALIZAR")
            {
                constructorCodigo.AppendLine(linea);
            }

            string codigo = constructorCodigo.ToString();

            if (!string.IsNullOrEmpty(codigo))
            {
                Lexer lexer = new Lexer(codigo, palabrasClaves);
                List<Token> tokens = lexer.AnalizarTokens();

                Console.WriteLine("\nTokens encontrados:");
                foreach (var token in tokens)
                {
                    if (token.Tipo != TipoToken.ESPACIO_BLANCO)
                    {
                        Console.WriteLine(token);
                    }
                }
            }

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}