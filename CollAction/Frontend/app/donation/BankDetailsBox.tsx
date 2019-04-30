﻿import * as React from "react";
import { IdealBankElement, IbanElement } from 'react-stripe-elements';

interface IBankDetailsBoxProps {
    stripe: any;
    amount: number;
    userEmail: string;
    userName: string;
    isRecurring: boolean;
}

interface IBankDetailsBoxState {
    showError: boolean;
    error: string;
}

export default class BankDetailsBox extends React.Component<IBankDetailsBoxProps, IBankDetailsBoxState> {
    constructor(props) {
        super(props);
        this.state = {
            showError: false,
            error: ""
        };
    }

    async submitPayment(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();
        this.setState({ showError: false });

        let sourceData = {
            type: "ideal",
            amount: this.props.amount * 100,
            currency: "eur",
            statement_descriptor: "Donation CollAction",
            owner: {
                name: this.props.userName,
                email: this.props.userEmail
            },
            redirect: {
                return_url: window.location.origin + "/donation/Return"
            }
        };

        let response = await this.props.stripe.createSource(sourceData);
        if (response.error) {
            console.log("Unable to start iDeal: " + response.error);
            this.setState({ showError: true, error: "Unable to start iDeal" })
            return;
        }

        let initializeUrl = `/donation/InitializeIdealCheckout?sourceId=${response.source.id}&name=${encodeURIComponent(this.props.userName)}&email=${encodeURIComponent(this.props.userEmail)}`;
        let initializeResponse = await fetch(initializeUrl, { method: 'POST' });
        if (initializeResponse.status == 200) {
            window.location.href = response.source.redirect.url;
        } else {
            console.log("Unable to start iDeal: " + await initializeResponse.text());
            this.setState({ showError: true, error: "Unable to start iDeal" })
        }
    }

    renderBankElement() {
        if (this.props.isRecurring) {
            return <IbanElement />;
        }
        else {
            return <IdealBankElement supportedCountries={['SEPA']} />
        }
    }

    render() {
        return (
            <form onSubmit={(ev) => this.submitPayment(ev)}>
                <div className="error" hidden={!this.state.showError}>
                    <p>{this.state.error}</p>
                </div>
                {this.renderBankElement()}
                <input type="submit" className="btn btn-default" value="submit" />
            </form>);
    }
} 