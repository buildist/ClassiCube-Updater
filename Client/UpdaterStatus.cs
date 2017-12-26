namespace ClassiCubeUpdater {
    class UpdaterStatus {
        public readonly Status Status;
        public readonly float Progress;
        public readonly string Message;

        public UpdaterStatus(Status status, float progress, string message) {
            this.Status = status;
            this.Progress = progress;
            this.Message = message;
        }
    }

    enum Status {
        NONE, IN_PROGRESS, ERROR, SUCCESS
    }
}
