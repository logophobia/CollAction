import React from "react";
import { Button } from "../../../components/Button/Button";
import { Section } from "../../../components/Section/Section";
import { Alert } from "../../../components/Alert/Alert";
import { Link, useLocation } from "react-router-dom";
import styles from "./Login.module.scss";
import { gql, useQuery } from "@apollo/client";

import {
  Grid,
  TextField,
  FormControl,
  FormControlLabel,
  Checkbox,
  FormGroup,
} from "@material-ui/core";

const GET_LOGIN_PROVIDERS = gql`
  query {
    miscellaneous {
      externalLoginProviders
    }
  }
`;

const LoginPage = () => {
  const actionLogin = `${process.env.REACT_APP_BACKEND_URL}/account/login`;
  const actionExternalLogin = `${process.env.REACT_APP_BACKEND_URL}/account/externalLogin`;
  const returnUrl = window.location.origin;
  const errorUrl = `${returnUrl}/account/login`;
  const searchParams = new URLSearchParams(useLocation().search);
  const errorType = searchParams.get("error");
  const errorMessage = searchParams.get("message");
  const { data: dataLoginProviders, error: errorLoginProviders } = useQuery(GET_LOGIN_PROVIDERS);
  if (errorType && errorMessage)
  {
    console.error({ errorType, errorMessage });
  }

  return (
    <React.Fragment>
      <Section className={styles.intro}>
        <h1 className={styles.title}>Login</h1>
        <h2 className={styles.subtitle}>
          (Use a local account to log in )
        </h2>
      </Section>
      <Alert type="error" text={errorMessage} />
      <Alert type="error" text={errorLoginProviders?.message} />
      <Section color="grey">
        <Grid container justify="center">
          <Grid item sm={6}>
            <form method="post" action={actionExternalLogin}>
              <FormGroup>
                <input
                  type="hidden"
                  name="returnUrl"
                  value={returnUrl}
                />
                <input type="hidden" name="errorUrl" value={errorUrl} />
                { dataLoginProviders?.miscellaneous?.externalLoginProviders?.map(
                  (provider: string) => (
                    <FormControl key={provider} margin="normal">
                      <Button name="provider" value={provider}>
                        Login with {provider}
                      </Button>
                    </FormControl>
                  )
                ) }
              </FormGroup>
            </form>
            <div className={styles.divider}>
              <span>or</span>
            </div>
            <form method="post" action={actionLogin}>
              <FormGroup>
                <FormControl margin="normal">
                  <TextField
                    name="Email"
                    className={styles.formControl}
                    label="E-mail"
                  />
                </FormControl>
                <FormControl margin="normal">
                  <TextField
                    name="Password"
                    className={styles.formControl}
                    label="Password"
                    type="password"
                  />
                </FormControl>
                <FormControlLabel
                  control={<Checkbox color="default" />}
                  label="Remember me"
                />
                <input
                  type="hidden"
                  name="returnUrl"
                  value={returnUrl}
                />
                <input type="hidden" name="errorUrl" value={errorUrl} />
                <Button type="submit">Login</Button>
                <Link to="/account/register-user">Register as new user</Link>
                <Link to="/account/forgot-password">I forgot my password</Link>
              </FormGroup>
            </form>
          </Grid>
        </Grid>
      </Section>
  </React.Fragment>
  );
}

export default LoginPage;