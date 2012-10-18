(function () {

    var findById = function (array, id) {
        return ko.utils.arrayFirst(array, function (item) { return item.Id == id; });
    };

    var gamesRoomHub = $.connection.gamesRoom;

    /// game engine event handlers

    //when you login
    gamesRoomHub.youLoggedIn = function (player, players, games) {
        $('#login').modal("hide"); //todo remove dependency on html
        model.loggedIn(true);
        resizeGameBoard(); //todo remove dependency on global function
        model.lookingForGame(true);
        model.id(player.Id);

        model.players(players); //load initial players
        model.games(games); //load initial games
    };

    //when any player login
    gamesRoomHub.playerLoggedIn = function (player) {
        model.players.push(player);
    };

    //when any player logouts
    gamesRoomHub.playerLoggedOut = function (player) {
        model.players.remove(findById(model.players(), player.Id));
    };

    //when anyone creates game
    gamesRoomHub.gameCreated = function (game) {
        model.games.push(game);
    };

    //when you create game
    gamesRoomHub.youCreatedGame = function () {
        model.clearGameBoard();

        model.waitingForOpponent(true);
        model.playing(false);
        model.lookingForGame(false);
    };

    //when anyone joins any game
    gamesRoomHub.gameJoined = function (game) {
        model.games.remove(findById(model.games(), game.Id));
        model.games.push(game);
    };

    //when you join game
    gamesRoomHub.youJoinedGame = function () {
        model.clearGameBoard();
    };

    //whem your game starts
    gamesRoomHub.yourGameStarted = function (game) {
        model.playing(true);
        model.waitingForOpponent(false);
        model.lookingForGame(false);

        model.gameId(game.Id);

        if (game.WhosTurn.Id == model.id()) {//if my turn
            model.myTurn(true);
            model.opponentsTurn(false);
        } else {
            model.myTurn(false);
            model.opponentsTurn(true);
        }
    };

    //when any game ends
    gamesRoomHub.gameEnded = function (game) {
        model.games.remove(findById(model.games(), game.Id));
    };

    //when your game ends
    gamesRoomHub.yourGameIsOver = function (game) {
        model.playing(false);
        model.waitingForOpponent(false);
        model.lookingForGame(true);

        model.gameId(null);

        model.myTurn(false);
        model.opponentsTurn(false);

        if (!game.IsDraw) {
            model.iWon(game.Winner.Id == model.id());
            model.iLose(game.Winner.Id != model.id());
        } else {
            model.draw(game.IsDraw);
        }
    };

    //when you or your opponent makes turn
    gamesRoomHub.turnMade = function (turn) {

        if (turn.PlayerId == model.id()) {
            model.myTurn(false);
            model.opponentsTurn(true);
        } else {
            model.myTurn(true);
            model.opponentsTurn(false);
        }

        if (turn.BoardMark === 0) {
            model.gameBoard[turn.Turn]("X");
        } else {
            model.gameBoard[turn.Turn]("O");
        }
    };

    /// game room view model

    function GameRoomModel() {
        var self = this;

        self.games = ko.observableArray([]);
        self.players = ko.observableArray([]);

        self.email = ko.observable("asdfsadfsdf@sdfsdff.df");
        self.nickname = ko.observable("asdfsghhh");
        self.id = ko.observable();
        self.gameId = ko.observable();

        self.gameBoard = new Array(9);
        for (var i = 0; i < 9; i++) {
            self.gameBoard[i] = ko.observable("");
        }

        //differen player states 
        self.loggedIn = ko.observable(false);
        self.playing = ko.observable(false);
        self.waitingForOpponent = ko.observable(false);
        self.lookingForGame = ko.observable(true);

        //more player states 
        self.iWon = ko.observable(false);
        self.iLose = ko.observable(false);
        self.draw = ko.observable(false);
        self.myTurn = ko.observable(false);
        self.opponentsTurn = ko.observable(false);

        self.showError = function(e) {
            alert(e);
        };

        self.clearGameBoard = function () {
            self.iWon(false);
            self.iLose(false);
            self.draw(false);
            self.myTurn(false);
            self.opponentsTurn(false);

            for (var i = 0; i < 9; i++) {
                self.gameBoard[i]("");
            }
        };

        self.createGame = function () {
            gamesRoomHub.createGame()
                .fail(function (e) {
                    self.showError(e);
                });
        };

        self.login = function () {
            gamesRoomHub.login(self.email(), self.nickname())
                .fail(function (e) {
                    self.showError(e);
                });
        };

        self.joinGame = function (game) {
            gamesRoomHub.joinGame(game.Id)
                .fail(function (e) {
                    self.showError(e);
                });
        };

        self.makeTurn = function (index) {
            gamesRoomHub.makeTurn(self.gameId(), index)
                .fail(function (e) {
                    self.showError(e);
                });
        };
    }

    var model = new GameRoomModel();
    ko.applyBindings(model);
    $.connection.hub.start();
})()