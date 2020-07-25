// Front-end game logic.
// Using jQuery and ES6 in browser due to time constraints.

document.addEventListener('DOMContentLoaded', () => {
    // HELPERS
    const letterFieldSelector = '.letter-container .letter';

    const sendGameRequest = (endpoint, method, successCallback, errorCallback, payload = {}) => {
        $.ajax({
            url: `Game/${endpoint}`,
            method: method,
            success: successCallback,
            error: errorCallback,
            data: payload
        });
    };

    /**
     * @param {string} message
     * @param {string} alertType bootstrap alert class (danger, warning, success, etc.)
     * @param {number} delay in ms
     */
    const showAlert = (message, alertType = 'danger', delay = 2000) => {
        const $alert = $('#alert');
        const alertClass = `alert-${alertType}`;
        
        if ($($alert).text()) {
            // some alert still displaying - try again later. 
            // this approach is far from perfect but it'll do for now
            setTimeout(() => showAlert(message, alertType, delay),500);
            return;
        } else {
            $($alert).addClass(alertClass).text(message);
        }

        if (delay !== Infinity) {
            $($alert).fadeIn(200, () =>
                setTimeout(() =>
                        $($alert).fadeOut(200,
                            () => $($alert).text(null).removeClass(alertClass)),
                    delay));
        } else {
            $($alert).addClass('permanent').fadeIn(200);
        }
    };

    const showErrorMessage = error => {
        let msg;
        if (error.responseJSON)
            msg = error.responseJSON.message;
        else if (error.statusText && error.statusText !== 'error')
            msg = error.statusText;
        else
            msg = `Something went wrong... Response status: ${error.status}`;

        showAlert(msg);
    };

    const toggleGuessInputDisabled = disabled => $('#guess').prop('disabled', disabled);

    const setLetter = letter => {
        const $availableCell = $(letterFieldSelector + ':empty').first();
        $($availableCell).text(letter);
    }

    const setLetterAndCheck = (letter) => {
        setLetter(letter);

        // check if all letters used up
        const $emptyLetterFields = $(letterFieldSelector + ':empty');
        if (!$emptyLetterFields.length) {
            // enable the 'guess' input
            toggleGuessInputDisabled(false);
        }
    };
    
    const noMoreRoundsLeft = () => {
        // disable all inputs
        toggleGuessInputDisabled(true);
        $('#btnVowel, #btnConsonant, #submit').prop('disabled', true);

        showAlert(
            'No more rounds left. Use "Reset game" button below to start new game',
            'primary',
            Infinity
        );
    }

    const populateWithState = ({currentLetters, round, score, isRoundLimitReached}) => {
        // reset current letters and inputs
        $(letterFieldSelector).text(null);
        toggleGuessInputDisabled(true);

        $.each(currentLetters.split(''), (i, l) => setLetterAndCheck(l));
        $('#round').text(round);
        $('#score').text(score);
        
        if (isRoundLimitReached){
            noMoreRoundsLeft();
        }
    }

    const getCurrentStateAndPopulate = () =>
        sendGameRequest('GetState', 'GET', populateWithState, showErrorMessage)

    // REGISTER EVENTS
    $('#guess').on('input', function(e) {
        // validate input
        const $submit = $('#submit');
        const valLen = this.value.length;
        
        if (valLen >= this.minLength && valLen <= this.maxLength){
            $($submit).prop('disabled', false);
        } else {
            $($submit).prop('disabled', true);
        }
    });

    $('#submit').on('click', function () {
        const onSuccess = ({longestWord, guessValid, state}) => {
            const $guess = $('#guess');
            const guessVal = $($guess).val();
            $($guess).val(null);

            const guessClass = guessValid ? 'success' : 'warning';
            const guessMessage = (guessValid
                ? 'Correct! Points have been awarded. '
                : `Sorry, "${guessVal}" is not a real word. `)
                + `FYI, the longest possible word was "${longestWord}"`;

            showAlert(guessMessage, guessClass, 6000);

            populateWithState(state);
        };

        const guess = $('#guess').val();
        sendGameRequest('SubmitGuessAndGetResult', 'POST', onSuccess, showErrorMessage, {guess});
    });

    // "reset game" button
    $('#reset').on('click', () => {
        const onSuccess = () => {
            // state has been reset - fetch new values
            getCurrentStateAndPopulate();
            showAlert('Game reset', 'success')
        };

        sendGameRequest('ResetGame', 'POST', onSuccess, showErrorMessage)
    });

    // "get letter" buttons
    $('#btnVowel, #btnConsonant').on('click', e => {
        const letterType = $(e.target).data('letterType');
        sendGameRequest(`GetLetter/${letterType}`, 'GET', setLetterAndCheck, showErrorMessage);
    });

    // INITIALISE GAME
    getCurrentStateAndPopulate();
});
