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

function setCookie (name, value) {
    document.cookie = name + "=" + encodeURIComponent(value);
}

function redirect(){
    window.location.replace("http://localhost:5000/Auth");
}

function isTokenExpired () {
    
        var urlParams = new URLSearchParams(window.location.search);
        var refreshTokenValue = urlParams.get('refreshToken');

    try {
        const token = localStorage.getItem('supabase.auth.token');
        if (!token) {
            console.log('Access token not found');
            return true;
        }

        const decodedToken = JSON.parse(atob(token.split('.')[1]));
        const tokenExpiry = new Date(decodedToken.exp * 1000);
        // const isExpired = Date.now() >= tokenExpiry;
        const specificDate = new Date('2020-01-01');
        const isExpired = specificDate <= tokenExpiry;
        
        console.log(isExpired);
        if (isExpired || refreshTokenValue === "True") {
            console.log('Access Token has expired');
            refreshAccessToken();
        }
        return isExpired;
    } catch (error) {
        console.error('Error getting the token: ', error);
        return true;
    }
}

async function refreshAccessToken () {
    const { data, error } = await supabase.auth.refreshSession();
    if (error) {
        console.error('Error refreshing session: ', error.message);
    } else {
        console.log('Access token refreshed successfully.');
        const newToken = data.access_token;
        setCookie('Token', newToken);
        setToken(data);
    }
}

function setToken () {
    const session = supabase.auth.session()
    var newToken = session.access_token;
    console.log(newToken);
    document.querySelector('#generated-token').value = newToken;
    console.log('New token set on the client side', newToken);
}

function ensureValidToken () {
    if (isTokenExpired()) {
        refreshAccessToken();
    }
}

ensureValidToken();