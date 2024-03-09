var SUPABASE_URL = 'https://zdgocunjwgxchxbxhytu.supabase.co'
var SUPABASE_KEY =
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpkZ29jdW5qd2d4Y2h4YnhoeXR1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDgyMzA4ODgsImV4cCI6MjAyMzgwNjg4OH0.nczlRY1bsS4_IJfdezci_uLL4tKag2bDgM0qTxDNcOc'

var supabase = supabase.createClient(SUPABASE_URL, SUPABASE_KEY)

function logInSubmitted() {
    const email = document.getElementById('log-in-email').value;
    const password = document.getElementById('log-in-password').value;

    supabase.auth
        .signIn({ email, password })
        .then((response) => {
            if (response.error) {
                document.getElementById('generated-token').value = response.error.message;
            } else {
                setToken(response);
                setCookie('Token', response.session.access_token);
                fetchUserDetails();
            }
        });
}

function fetchUserDetails () {
    document.getElementById('user-info').innerText = JSON.stringify(supabase.auth.user(), null, 4);
}

function setToken (response) {
    const newToken = response.data.access_token;
    document.querySelector('#generated-token').value = newToken;
    console.log('New token set on the client side', newToken);
}

function setCookie (name, value) {
    document.cookie = name + "=" + encodeURIComponent(value);
}

function redirect(){
    window.location.replace("http://localhost:5000/Auth");
}

function isTokenExpired () {
    const tokenExpiry = localStorage.getItem('supabase.auth.token');
    const isExpired = tokenExpiry && Date.now() >= tokenExpiry;
    if (isExpired) {
        console.log('Access Token has expired');
    }
    return isExpired;
}

async function refreshAccessToken () {
    const {data, error} = await supabase.auth.refreshSession();
    if (error) {
        console.error('Error refreshing session: ', error.message);
    } else {
        console.log('Access token refreshed successfully.');
    }
}

function ensureValidToken () {
    if (isTokenExpired()) {
        refreshAccessToken();
    }
}

ensureValidToken();