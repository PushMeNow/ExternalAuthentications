function onSignIn(googleUser) {
    let profile = googleUser.getBasicProfile();
    console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
    console.log('Name: ' + profile.getName());
    console.log('Image URL: ' + profile.getImageUrl());
    console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.
}

function signOut() {
    let auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut().then(function () {
        console.log('User signed out.');
    });
}

function getCurrentUserInfo() {
    return gapi.auth2.getAuthInstance().currentUser.get();
}

function getUserProfile() {
    return getCurrentUserInfo().getBasicProfile()
}

// return model { expires_at, expires_in, first_issued_at, id_token, idpId, login_hint, session_state, token_type }.
function getAuthInfo() {
    let currentUser = getCurrentUserInfo();

    return currentUser.getAuthResponse();
}

function getAuthHeader() {
    return {
        Authorization: "Google " + getAuthInfo().id_token
    };
}