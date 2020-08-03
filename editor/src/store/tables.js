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
  })
  .post({
    property: "tables",
    action: "addPage",
    path: ({ link }) => `/tables/add/?link=${link}`,
    onSuccess(state, payload, axios, { params, data }) {
      let testData = payload.data.replace(/^\"*/, "").replace(/\"*$/, "");
      console.log(testData);
      state.tables = JSON.parse(testData);
    },
  })
  .post({
    property: "tables",
    action: "clear",
    path: "/tables/clear/",
  })
  .getStore();

export default tables;
