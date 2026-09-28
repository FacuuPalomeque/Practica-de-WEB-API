 public class Resolucion
    {
        private int id;
        private int participanteId;
        private int desafioId;
        private double puntajeObtenido;
        private DateTime fechaEntrega;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int ParticipanteId
        {
            get { return participanteId; }
            set { participanteId = value; }
        }

        public int DesafioId
        {
            get { return desafioId; }
            set { desafioId = value; }
        }

        public double PuntajeObtenido
        {
            get { return puntajeObtenido; }
            set { puntajeObtenido = value; }
        }

        public DateTime FechaEntrega
        {
            get { return fechaEntrega; }
            set { fechaEntrega = value; }
        }
    }
