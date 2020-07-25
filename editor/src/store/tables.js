import Vapi from "vuex-rest-api";

const tables = new Vapi({
  baseURL: "http://localhost/",
  state: {
    tables: [],
  },
})
  .get({
    action: "getTables",
    property: "tables",
    path: "/tables",
    onSuccess(state, payload, axios, { params, data }) {
      console.log("payload");
      state.tables = payload.data;
      console.log(`Posts successfully fetched.`);
    },
    onError(state, error, axios, { params, data }) {
      console.log("error fetch");
      state.tables = null;
    },
  })
  .getStore();

export default tables;
