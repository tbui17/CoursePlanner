import { readFile } from "fs/promises";

const envVars = await readFile(".env", "utf-8");
