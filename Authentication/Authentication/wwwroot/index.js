var SUPABASE_URL = 'https://zdgocunjwgxchxbxhytu.supabase.co'
var SUPABASE_KEY =
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpkZ29jdW5qd2d4Y2h4YnhoeXR1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDgyMzA4ODgsImV4cCI6MjAyMzgwNjg4OH0.nczlRY1bsS4_IJfdezci_uLL4tKag2bDgM0qTxDNcOc'

var supabase = supabase.createClient(SUPABASE_URL, SUPABASE_KEY)

async function logInSubmitted () {
    const email = document.getElementById('log-in-email').value;
    const password = document.getElementById('log-in-password').value;

    const { user, session, error} = await supabase.auth.signIn({
        email: email,
        password: password
    })

    if (error) {
        document.getElementById('generated-token').value = 'Error logging in: ' + error.message;
        console.error('Error logging in:', error)
    }

    setToken(session.access_token);
    setCookie('Token', session.access_token);
    fetchUserDetails();
    
    // The initial access token and refresh token
    console.log('Initial Access Token:', session.access_token)
    console.log('Initial Refresh Token:', session.refresh_token)
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
        const session = supabase.auth.session()
        if (!session) {
            console.log('No active session');
            return true;
        }

        const decodedToken = JSON.parse(atob(session.access_token.split('.')[1]))
        const tokenExpiry = new Date(decodedToken.exp * 1000);
        const isExpired = Date.now() >= tokenExpiry;
        // const specificDate = new Date('2020-01-01')
        // const isExpired = specificDate <= tokenExpiry

        console.log('Is token expired: ', isExpired)

        if (isExpired || refreshTokenValue === "True") {
            console.log('Access Token has expired')
            refreshAccessToken(session.refresh_token)
        }
        return isExpired;
    } catch (error) {
        console.error('Error getting the token: ', error);
        return true;
    }
}

async function refreshAccessToken (refreshToken) {
    const {data, error} = await supabase.auth.refreshSession(refreshToken)

    if (error) {
        console.error('Error refreshing session: ', error)
    } else {
        // The new access token and refresh token
        console.log('Access token refreshed successfully.');
        setCookie('Token', data.access_token);
        setToken(data.access_token);
        console.log('New Access Token:', data.access_token);
        console.log('New Refresh Token:', data.refresh_token);
    }
}

function setToken (token) {
    console.log('New token set on the client side', token);
    document.querySelector('#generated-token').value = token;
}

function ensureValidToken () {
    if (isTokenExpired()) {
        const session = supabase.auth.session();
        refreshAccessToken(session.refresh_token)
    }
}

// ensureValidToken();

function getProducts() {
    axios.get("/Products")
        .then(function (response) {
                const products = response.data;
                const session = supabase.auth.session();
                console.log('new session.access_token: ', session.access_token)
                console.log(`GET Products: `, products);
        })
        .catch(function (error) {
            console.error('Error getting products: ', error)
            if (error.response.status === 403 && error.response.data === 'Token Expired') {
                const session = supabase.auth.session();
                supabase.auth.refreshSession(session.refresh_token)
                    .then(function (result) {
                        var newAccessToken = result.data.access_token
                        console.log('New Access Token: ', newAccessToken)
                        setCookie('Token', newAccessToken);
                        setToken(newAccessToken);
                        console.log('new session.access_token: ', session.access_token)
                        // Show the products retrieved initially
                        axios.get("/Products")
                            .then(function (refreshedResponse) {
                                const refreshedProducts = refreshedResponse.data;
                                console.log('GET Products after token refresh: ', refreshedProducts);
                            })
                    })
                    .catch(function (error) {
                        console.error('Error refreshing session: ', error);
                    })
            }
        })
}